using DotLiquid;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace GMF.Transform
{
   public static class LiquidConverter
    {
	public static string GivenALiquidTemplateText()
	{
		return @"<?xml version=""1.0"" encoding=""UTF-8""?>
<CPW DateTime=""{{Inbound.transmissionDateTime}}"" sourceBatchID=""{{Inbound.sourceBatchID}}"" ssn=""{{Inbound.ssn}}"" pageCount=""{{Inbound.pageCount}}""
schemaVersion =""{{Inbound.schemaVersion}}"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:fn=""http://www.w3.org/TR/xpath-functions/"">
		<Contract>
			{%- for vehicle in Inbound.Case.Vehicles.Vehicle -%}
				{%- if vehicle.Model == ""Malibu"" -%}
			<Vehicle>
				<Year>{{vehicle.Year}}</Year>
				<Make>{{vehicle.Make}}</Make>
				<Model>{{vehicle.Model}}</Model>
			</Vehicle >
				{%- endif -%}
			{%- endfor -%}	
			<Pricing>
			{%- for loan in Inbound.Case.Loans.LoanInfo -%}
				<Price>
					<ApprovalType>{{loan.loan_type}}</ApprovalType>
					<Program>{{loan.program}}</Program>
					<Rate>{{loan.percentAmount}}</Rate>
				</Price>
			{%- endfor -%}	
			</Pricing>	
			<Financing>
				<MonthlyTotal>{{Inbound.Case.fin.total}}</MonthlyTotal>
				<StartDate>{{Inbound.Case.fin.mth_st_dt}}</StartDate>
				<MonthlyAmount>{{Inbound.Case.fin.mth_pay_amt}}</MonthlyAmount>
				<Term>{{Inbound.Case.fin.term}}</Term>
			</Financing>
		</Contract >
	
		<Partner sourceCode=""{{Inbound.sourceCode}}"">
			<DataFilePath>{{Inbound.metaDataFilePath}}</DataFilePath>
			<appNumber>{{Inbound.applicationNumber}}</appNumber>
			<PropductType>{{Inbound.contractType}}</PropductType>
		 </Partner>
	 </CPW>
	 ";

	}
	public static string ConvertFromXml(string source, string rootName, string template)
        {
			string output = string.Empty;
			try
			{
				using (var stream = new System.IO.MemoryStream(
					Encoding.UTF8.GetBytes(source)
					))
				{
					XElement root = XDocument.Load(stream).Element(rootName);
					output = ConvertFromXml(root, template);
				}
			}
			catch(Exception ex)
			{
				output = ex.Message;
			}
			return output;
        }

        public static string ConvertFromXml(XElement root, string template)
        {
            var content = XmlConverter.ToDynamic(root);
            var json = JsonConvert.SerializeObject(content);
            var input = JsonConvert.DeserializeObject<IDictionary<string, object>>(
                json,
                new JsonToDictionaryConverter());
            var liquidTemplate = Template.Parse(template);
            return liquidTemplate.Render(Hash.FromDictionary(input));

        }

        public static string ConvertFromJson(string json, string template)
        {
            var input = JsonConvert.DeserializeObject<IDictionary<string, object>>(
                            json,
                            new JsonToDictionaryConverter());
            var liquidTemplate = Template.Parse(template);
            return liquidTemplate.Render(Hash.FromDictionary(input));

        }
    }
}
