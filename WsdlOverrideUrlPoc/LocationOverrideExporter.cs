using System;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Web;
using System.Web.Services.Description;
using System.Xml.Schema;
using ServiceDescription = System.Web.Services.Description.ServiceDescription;

namespace WsdlOverrideUrlPoc
{
    class LocationOverrideExporter
    {
        Uri location;
        Dictionary<object, string> queryFromDoc = new Dictionary<object, string>();

     

        public void ExportEndpoint(WsdlExporter exporter, WsdlEndpointConversionContext context)
        {
            foreach (object extension in context.WsdlPort.Extensions)
            {
                SoapAddressBinding addr = (extension as SoapAddressBinding);
                if (addr != null)
                {
                    var scheme = HttpContext.Current.Request.Headers["X-Forwarded-Proto"] ?? HttpContext.Current.Request.Url.Scheme;
                    var host = HttpContext.Current.Request.Url.Host;
                    var existingUri = new Uri(addr.Location);
                    var newUri = new Uri(new Uri($"{scheme}://{host}"), existingUri.PathAndQuery);
                    addr.Location = newUri.ToString();
                }
            }

            EnumerateWsdlsAndSchemas(exporter.GeneratedWsdlDocuments, exporter.GeneratedXmlSchemas);

            foreach (ServiceDescription description in exporter.GeneratedWsdlDocuments)
            {
                FixImportAddresses(exporter.GeneratedWsdlDocuments, description, exporter.GeneratedXmlSchemas);
            }
            foreach (XmlSchema schema in exporter.GeneratedXmlSchemas.Schemas())
            {
                FixImportAddresses(exporter.GeneratedXmlSchemas, schema);
            }
        }

        private void EnumerateWsdlsAndSchemas(ServiceDescriptionCollection wsdls, XmlSchemaSet xsds)
        {
            ServiceDescription rootDescription = WsdlUtils.FindRootDescription(wsdls);

            int num = 0;
            foreach (ServiceDescription description2 in wsdls)
            {
                string key = "wsdl";
                if (description2 != rootDescription)
                {
                    key = key + "=wsdl" + num++;
                }
                queryFromDoc.Add(description2, key);
            }
            int num2 = 0;
            foreach (XmlSchema schema in xsds.Schemas())
            {
                string str2 = "xsd=xsd" + num2++;
                queryFromDoc.Add(schema, str2);
            }
        }

        private void FixImportAddresses(ServiceDescriptionCollection wsdls, ServiceDescription wsdlDoc, XmlSchemaSet schemas)
        {
            foreach (Import import in wsdlDoc.Imports)
            {
                if (string.IsNullOrEmpty(import.Location))
                {
                    ServiceDescription description = wsdls[import.Namespace ?? string.Empty];
                    if (description != null)
                    {
                        string query = queryFromDoc[description];
                        import.Location = this.location + "?" + query;
                    }
                }
            }
            if (wsdlDoc.Types != null)
            {
                foreach (XmlSchema schema in wsdlDoc.Types.Schemas)
                {
                    this.FixImportAddresses(schemas, schema);
                }
            }
        }

        private void FixImportAddresses(XmlSchemaSet xmlSchemaSet, XmlSchema xsdDoc)
        {
            foreach (XmlSchemaExternal external in xsdDoc.Includes)
            {
                if ((external != null) && string.IsNullOrEmpty(external.SchemaLocation))
                {
                    string str = (external is XmlSchemaImport) ? ((XmlSchemaImport)external).Namespace : xsdDoc.TargetNamespace;
                    foreach (XmlSchema schema in xmlSchemaSet.Schemas(str ?? string.Empty))
                    {
                        if (schema != xsdDoc)
                        {
                            string query = queryFromDoc[schema];
                            external.SchemaLocation = this.location + "?" + query;
                            break;
                        }
                    }
                    continue;
                }
            }
        }


    }
}
