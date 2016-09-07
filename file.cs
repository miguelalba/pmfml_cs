using System.Collections.Generic;
using System.Xml;

using pmfml_cs.model;
using pmfml_cs.numl;
using LibCombine;

namespace pmfml_cs
{
    class URI
    {
        public static string NUML = "https://raw.githubusercontent.com/NuML/NuML/master/NUMLSchema.xsd";
        public static string PMF = "http://sourceforge.net/projects/microbialmodelingexchange/files/";
        public static string SBML = "http://identifiers.org/combine/specifications/sbml";
    }

    public class PmfMetaDataNode
    {
        private static string MODEL_TYPE = "modelType";
        private static string MASTER_FILE = "masterFile";

        public ModelType modelType { get; }
        public HashSet<string> masterFiles;
        public XmlElement element { get; }

        public PmfMetaDataNode(ModelType modelType, HashSet<string> masterFiles)
        {
            this.modelType = modelType;
            this.masterFiles = masterFiles;

            XmlDocument tempDoc = new XmlDocument();  // auxiliary document for creating  XmlElements
            element = tempDoc.CreateElement("metaParent");

            XmlElement modelTypeElement = tempDoc.CreateElement(MODEL_TYPE);
            modelTypeElement.InnerText = modelType.name();
            element.AppendChild(modelTypeElement);

            foreach (string masterFile in masterFiles)
            {
                XmlElement masterFileElement = tempDoc.CreateElement(MASTER_FILE);
                masterFileElement.InnerText = masterFile;
                element.AppendChild(masterFileElement);
            }
        }

        public PmfMetaDataNode(XmlElement element)
        {
            this.element = element;

            modelType = ModelType.ValueOf(element.SelectSingleNode(MODEL_TYPE).InnerText);
            XmlNodeList masterFileElements = element.GetElementsByTagName(MASTER_FILE);

            masterFiles = new HashSet<string>();
            for (int i = 0; i < masterFileElements.Count; i++)
            {
                masterFiles.Add(masterFileElements[i].InnerText);
            }
        }
    }

    public class ExperimentalDataFile
    {
        private ExperimentalDataFile() { }

        public static List<ExperimentalData> readPMF(string filename) { return read(filename); }
        public static List<ExperimentalData> readPMFX(string filename) { return read(filename); }

        public static void writePMF(string dir, string filename, List<ExperimentalData> dataRecords)
        {
            string caName = dir + "/" + filename + ".pmf";
            write(caName, dataRecords);
        }

        public static void writePMFX(string dir, string filename, List<ExperimentalData> dataRecords)
        {
            string caName = dir + "/" + filename + ".pmfx";
            write(caName, dataRecords);
        }

        private static List<ExperimentalData> read(string filename)
        {
            CombineArchive combineArchive = CombineArchive.FromFile(filename);
            List<ExperimentalData> dataRecords = new List<ExperimentalData>();

            foreach (Entry entry in combineArchive.GetEntriesWithFormat(URI.NUML))
            {
                string docName = entry.GetLocalFileName();
                NuMLDocument doc = NuMLReader.read(docName);
                dataRecords.Add(new ExperimentalData(docName, doc));
            }

            return dataRecords;
        }

        public static void write(string filename, List<ExperimentalData> dataRecords)
        {
            CombineArchive combineArchive = new CombineArchive();
            combineArchive.ArchiveFileName = filename;

            // Add data records
            foreach (ExperimentalData ed in dataRecords)
            {
                // TODO: give tmpFile the ed.docName instead of random file name
                string tmpFile = System.IO.Path.GetTempFileName();
                NuMLWriter.write(ed.doc, tmpFile);
                combineArchive.AddEntry(tmpFile, URI.NUML, null);
            }
        }
    }

    // PrimaryModelWDataFile
    public class PrimaryModelWDataFile
    {
        public static List<PrimaryModelWData> readPMF(string filename) { return read(filename, URI.SBML); }
        public static List<PrimaryModelWData> readPMFX(string filename) { return read(filename, URI.PMF); }

        public static void writePMF(string dir, string filename, List<PrimaryModelWData> models)
        {
            string caName = dir + "/" + filename + ".pmf";
            write(caName, URI.SBML, models);
        }

        public static void writePMFX(string dir, string filename, List<PrimaryModelWData> models)
        {
            string caName = dir + "/" + filename + ".pmf";
            write(caName, URI.PMF, models);
        }

        private static List<PrimaryModelWData> read(string filename, string uri)
        {
            CombineArchive combineArchive = CombineArchive.FromFile(filename);

            List<PrimaryModelWData> models = new List<>();

            // Gets data entries

            // ...
        }
    }

    // PrimaryModelWODataFile
    // TwoStepSecondaryModelFile
    // OneStepSecondaryModelFile
    // ManualSecondaryModelFile
    // TwoStepTertiaryModelFile
    // OneStepTertiaryModelFile
    // ManualTertiaryModelFile
}
