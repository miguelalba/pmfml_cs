using System;
using System.Collections.Generic;

using libsbmlcs;

namespace pmfml_cs.model
{

    /// <summary>
    /// Case 0. Each document is a NuMLDocument that keeps a time series.
    /// </summary>
    public class ExperimentalData
    {
        public string docName { get; }
        public NuMLDocument doc { get; }

        public ExperimentalData(string docName, NuMLDocument doc)
        {
            this.docName = docName;
            this.doc = doc;
        }
    }

    /// <summary>
    /// Case 1a. Each primary model includes a NuMLDocument.
    /// </summary>
    public class PrimaryModelWData
    {
        public string modelDocName { get; }
        public SBMLDocument modelDoc { get; }

        public string dataDocName { get; }
        public NuMLDocument dataDoc { get; }

        public PrimaryModelWData(string modelDocName, SBMLDocument modelDoc,
            string dataDocName, NuMLDocument dataDoc)
        {
            this.modelDocName = modelDocName;
            this.modelDoc = modelDoc;

            this.dataDocName = dataDocName;
            this.dataDoc = dataDoc;
        }
    }

    /// <summary>
    /// Case 1b. Primary model without data files.
    /// </summary>
    public class PrimaryModelWOData
    {
        public string docName { get; }
        public SBMLDocument doc { get; }

        public PrimaryModelWOData(string docName, SBMLDocument doc)
        {
            this.docName = docName;
            this.doc = doc;
        }
    }

    /// <summary>
    /// Case 2a. Includes an SBMLDocument for the secondary model (master) and n
    /// primary models with data.
    /// </summary>
    public class TwoStepSecondaryModel
    {
        public SBMLDocument secDoc { get; }  // secondary model (master) document
        public string secDocName { get; }

        public List<PrimaryModelWData> primModels;  // primary models with data

        public TwoStepSecondaryModel(string secDocName, SBMLDocument secDoc,
            List<PrimaryModelWData> primModels)
        {
            this.secDocName = secDocName;
            this.secDoc = secDoc;
            this.primModels = primModels;
        }
    }

    /// <summary>
    /// Case 2b: It has an SBMLDocuemtn for a secondary model and its primary
    /// model, which can be linked to n NuMLDocument.
    /// </summary>
    public class OneStepSecondaryModel
    {
        public string modelDocName { get; }
        public SBMLDocument modelDoc { get; }  // Document with primary and secondary models

        public List<string> dataDocNames { get; }
        public List<NuMLDocument> dataDocs { get; }  // Data documents

        public OneStepSecondaryModel(string modelDocName, SBMLDocument modelDoc,
            List<string> dataDocNames, List<NuMLDocument> dataDocs)
        {
            this.modelDocName = modelDocName;
            this.modelDoc = modelDoc;
            this.dataDocNames = dataDocNames;
            this.dataDocs = dataDocs;
        }
    }

    /// <summary>
    /// Case 2c. Holds secondary models generated manually with a SBMLDocument
    /// per each secondary model.
    /// </summary>
    public class ManualSecondaryModel
    {
        public string docName { get; }
        public SBMLDocument doc { get; }

        public ManualSecondaryModel(string docName, SBMLDocument doc)
        {
            this.docName = docName;
            this.doc = doc;
        }
    }

    /// <summary>
    /// Case 3a. Keeps one SBMLDocument per each tertiary model, linked to n SBMLDocuments
    /// for the secondary models. The SBMLDocument of the tertiary model also links to a
    /// number of data files.
    /// </summary>
    public class TwoStepTertiaryModel
    {

        public string tertDocName { get; }
        public SBMLDocument tertDoc { get; }

        public List<PrimaryModelWData> primModels { get; }

        public List<string> secDocNames { get; }
        public List<SBMLDocument> secDocs { get; }

        public TwoStepTertiaryModel(string tertDocName, SBMLDocument tertDoc,
            List<PrimaryModelWData> primModels, List<string> secDocNames,
            List<SBMLDocument> secDocs)
        {
            this.tertDocName = tertDocName;
            this.tertDoc = tertDoc;

            this.primModels = primModels;

            this.secDocNames = secDocNames;
            this.secDocs = secDocs;
        }
    }

    /// <summary>
    /// Case 3b: Keeps one SBMLDocument per each tertiary model, linked to n
    /// SBMLDocuments for the secondary models. The SBMLDocument of the tertiary model
    /// also links to a number of data files.
    /// </summary>
    public class OneStepTertiaryModel
    {
        public string tertDocName { get; }
        public SBMLDocument tertDoc { get; }

        public List<string> secDocNames { get; }
        public List<SBMLDocument> secDocs { get; }

        public List<string> dataDocNames { get; }
        public List<NuMLDocument> dataDocs { get; }

        public OneStepTertiaryModel(string tertDocName, SBMLDocument tertDoc,
            List<string> secDocNames, List<SBMLDocument> secDocs,
            List<string> dataDocNames, List<NuMLDocument> dataDocs)
        {
            this.tertDocName = tertDocName;
            this.tertDoc = tertDoc;

            this.secDocNames = secDocNames;
            this.secDocs = secDocs;

            this.dataDocNames = dataDocNames;
            this.dataDocs = dataDocs;
        }
    }

    /// <summary>
    /// Case 3c. Keeps a master SBMLDocument per tertiary model linked to n SBMLDocuments
    /// for the secondary models. It has no data.
    /// </summary>
    public class ManualTertiaryModel
    {
        public string tertDocName { get; }
        public SBMLDocument tertDoc { get; }

        public List<string> secDocNames { get; }
        public List<SBMLDocument> secDocs { get; }

        public ManualTertiaryModel(string tertDocName, SBMLDocument tertDoc,
            List<string> secDocNames, List<SBMLDocument> secDocs)
        {
            this.tertDocName = tertDocName;
            this.tertDoc = tertDoc;
            this.secDocNames = secDocNames;
            this.secDocs = secDocs;
        }
    }
}
