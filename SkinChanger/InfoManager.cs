using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace SkinChanger
{
    internal static class InfoManager
    {
        internal struct Model
        {
            public readonly string Name;
            public readonly ModelSkin[] Skins;

            public Model(string name, ModelSkin[] skins)
            {
                Name = name;
                Skins = skins;
            }

            public string[] GetSkinNames()
            {
                return Skins.Select(skin => skin.Name).ToArray();
            }
        }

        internal struct ModelSkin
        {
            public readonly string Name;
            public readonly int Index;

            public ModelSkin(string name, string index)
            {
                Name = name;
                Index = int.Parse(index);
            }
        }

        public static string[] ModelNames;

        private static XmlDocument _infoXml;
        private static Model[] _models;

        public static void Initialize()
        {
            using (var infoStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SkinChanger.Info.xml"))
            // ReSharper disable once AssignNullToNotNullAttribute
            using (var infoReader = new StreamReader(infoStream))
            {
                _infoXml = new XmlDocument();
                _infoXml.LoadXml(infoReader.ReadToEnd());
            }

            // The following lines are ReSharper-simplification magic.
            _models =
                // ReSharper disable once PossibleNullReferenceException
                _infoXml.DocumentElement.ChildNodes.Cast<XmlElement>()
                    .Select(
                        model =>
                            new Model(model.Attributes["name"].Value,
                                model.ChildNodes.Cast<XmlElement>()
                                    .Select(
                                        skin =>
                                            new ModelSkin(skin.Attributes["name"].Value, skin.Attributes["index"].Value))
                                    .ToArray()))
                    .ToArray();

            ModelNames = _models.Select(model => model.Name).ToArray();
        }

        public static Model GetModelByIndex(int index)
        {
            return _models[index];
        }

        public static Model GetModelByName(string name)
        {
            return
                _models.FirstOrDefault(
                    model => string.Equals(model.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
