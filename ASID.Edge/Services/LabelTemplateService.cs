using System.Collections.Generic;
using System.IO;

namespace ASID.Edge.Services
{
    public class LabelTemplateService
    {
        public string LoadTemplate(
            string templateName,
            Dictionary<string, string> tokens)
        {
            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Labels",
                templateName);

            string zpl = File.ReadAllText(path);

            foreach (var token in tokens)
            {
                zpl = zpl.Replace(
                    "{" + token.Key + "}",
                    token.Value);
            }

            return zpl;
        }
    }
}