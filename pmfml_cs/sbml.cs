/***************************************************************************************************
 * Copyright (c) 2015 Federal Institute for Risk Assessment (BfR), Germany
 *
 * This program is free software: you can redistribute it and/or modify it under the terms of the
 * GNU General Public License as published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without
 * even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with this program. If
 * not, see <http://www.gnu.org/licenses/>.
 *
 * Contributors: Department Biological Safety - BfR
 **************************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using libsbmlcs;

namespace pmfml_cs.sbml
{
    public class CondIdNode
    {
        public const string NS = "pmmlab";
        public const string TAG = "condID";

        XMLNode node;

        public CondIdNode(int id)
        {
            node = new XMLNode(new XMLTriple(TAG, null, NS));
            node.addChild(new XMLNode(id.ToString()));
        }

        public CondIdNode(XMLNode node)
        {
            this.node = node;
        }

        public int getCondId()
        {
            return int.Parse(node.getChild(0).getCharacters());
        }

        public XMLNode getNode() { return node; }
    }

    public class Correlation
    {
        private string name;
        private double value;  // worths NaN when not set

        public Correlation(string name)
        {
            this.name = name;
            this.value = double.NaN;
        }

        public Correlation(string name, double value)
        {
            this.name = name;
            this.value = value;
        }

        public string getName() { return name; }
        public double getValue() { return value; }

        public bool isSetValue()
        {
            return !double.IsNaN(value);
        }

        public override string ToString()
        {
            return "Correlation [name=" + name + ", value=" + value + "]";
        }

        // maybe equals should be implemented here
    }

    class DataSourceNode
    {
        public const string TAG = "dataSource";
        public const string NS = "pmmlab";

        XMLNode node;

        public DataSourceNode(XMLNode node)
        {
            this.node = node;
        }

        public DataSourceNode(string dataName)
        {
            XMLTriple triple = new XMLTriple(TAG, null, NS);

            XMLAttributes attrs = new XMLAttributes();
            attrs.add("id", "source1");
            attrs.add("href", dataName);

            node = new XMLNode(triple, attrs);
        }

        public string getFile()
        {
            return node.getAttrValue("href");
        }

        public XMLNode getNode()
        {
            return node;
        }
    }

    class GlobalModelIdNode
    {
        public const string NS = "pmmlab";
        public const string TAG = "globalModelID";

        // TODO: continue
        XMLNode node;

        public GlobalModelIdNode(int id)
        {
            node = new XMLNode(new XMLTriple(TAG, null, NS));
            node.addChild(new XMLNode(id.ToString()));
        }

        public GlobalModelIdNode(XMLNode node)
        {
            this.node = node;
        }

        public int getGlobalModelId()
        {
            return int.Parse(node.getChild(0).getCharacters());
        }

        public XMLNode getNode()
        {
            return node;
        }
    }

    /// <summary>Limit values of a parameter.</summary>
    class Limits
    {
        private string var;
        private double min;
        private double max;

        /// <summary>Creates new Limits of a variable.</summary>
        /// <param name="var">Variable name</param>
        /// <param name="min">Variable minimum value</param>
        /// <param name="max">Variable maximum value</param>
        public Limits(string var, double min, double max)
        {
            this.var = var;
            this.min = min;
            this.max = max;
        }

        public string getVar() { return var; }
        public double getMin() { return min; }
        public double getMax() { return max; }
    }

    class LimitsConstraint
    {
        const int LEVEL = 3;
        const int VERSION = 1;
        Constraint constraint;

        /// <summary>
        /// Initializes constraint. var cannot be null. Either min or can be NaN
        /// but not both.
        /// </summary>
        /// <param name="var">Variable name</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        public LimitsConstraint(string var, double min, double max)
        {
            string formula;
            if (!double.IsNaN(min) && !double.IsNaN(max))
            {
                formula = "(" + var + " >= " + min + ") && (" + var + " <= " + max + ")";
            }
            else if (!double.IsNaN(min))
            {
                formula = "(" + var + " >= " + min + ")";
            }
            else if (!double.IsNaN(max))
            {
                formula = "(" + var + " <= " + max + ")";
            }
            else
            {
                formula = "";
            }

            if (!string.IsNullOrEmpty(formula))
            {
                // TODO: Parse formula and create constraint
                ASTNode math = libsbml.parseFormula(formula);
                constraint = new Constraint(LEVEL, VERSION);
                constraint.setMath(math);
            }
        }

        /// <summary>Creates a LimitConstraint from existing constraint.</summary>
        /// <param name="constraint">SBML Constraint</param>
        public LimitsConstraint(Constraint constraint)
        {
            this.constraint = constraint;
        }

        public Constraint getConstraint()
        {
            return constraint;
        }

        public Limits getLimits()
        {
            ASTNode math = constraint.getMath();
            ASTNodeList nodes = math.getListOfNodes();

            string var;
            double min = double.NaN;
            double max = double.NaN;

            // Constraint with a single condition
            if (!math.ToString().Contains("&&"))
            {
                var = nodes.get(0).getName();
                double val = double.Parse(nodes.get(1).ToString());

                // Figure out whether val is a min or max, according to the
                // math's type
                int type = constraint.getMath().getType();

                // var <= val ==> val is a maximum
                if (type == libsbml.AST_RELATIONAL_LEQ || type == libsbml.AST_RELATIONAL_LT)
                {
                    max = val;
                }

                // else: var >= val ==> val is a minimum
                else
                {
                    min = val;
                }
            }

            // Constraint with two conditions. E.g. [mu_max >= 0, mu_max <= 5]
            else
            {
                // Get minimum from the left node (mu_max >= 0)
                ASTNode leftNode = nodes.get(0);
                ASTNodeList leftNodes = leftNode.getListOfNodes();
                var = leftNodes.get(0).getName();
                min = double.Parse(leftNodes.get(1).ToString());  // min (0)

                // Get maximum from the right node (mu_max <= 5)
                ASTNode rightNode = nodes.get(1);
                ASTNodeList rightNodes = rightNode.getListOfNodes();
                max = double.Parse(rightNodes.get(1).ToString());  // max (5)
            }

            return new Limits(var, min, max);
        }
    }

    /// <summary>Holds meta data related to a model.</summary>
    public interface Metadata
    {
        string getGivenName();
        void setGivenName(string name);
        bool isSetGivenName();

        string getFamilyName();
        void setFamilyName(string name);
        bool isSetFamilyName();

        string getContact();
        void setContact(string contact);
        bool isSetContact();

        string getCreatedDate();
        void setCreatedDate(string date);
        bool isSetCreatedDate();

        string getModifiedDate();
        void setModifiedDate(string date);
        bool isSetModifiedDate();

        ModelType getType();
        void setType(ModelType type);
        bool isSetType();

        string getRights();
        void setRights(string rights);
        bool isSetRights();

        string getReferenceLink();
        void setReferenceLink(string link);
        bool isSetReferenceLink();
    }

    class MetadataImpl : Metadata
    {
        private const string GIVEN_NAME = "givenName";
        private const string FAMILY_NAME = "familyName";
        private const string CONTACT = "contact";
        private const string CREATED_DATE = "createdDate";
        private const string MODIFIED_DATE = "modifiedDate";
        private const string TYPE = "type";
        private const string RIGHTS = "rights";
        private const string REFERENCE_LINK = "referenceLink";

        private Hashtable ht;

        public MetadataImpl()
        {
            ht = new Hashtable(8);
        }

        public MetadataImpl(string givenName, string familyName, string contact,
            string createdDate, string modifiedDate, ModelType type,
            string rights, string referenceLink)
        {
            ht = new Hashtable(8);

            setGivenName(givenName);
            setFamilyName(familyName);
            setContact(contact);
            setCreatedDate(createdDate);
            setModifiedDate(modifiedDate);
            setType(type);
            setRights(rights);
            setReferenceLink(referenceLink);
        }

        // --- given name ---
        public string getGivenName()
        {
            return (string)ht[GIVEN_NAME];
        }

        public void setGivenName(string givenName)
        {
            if (!string.IsNullOrEmpty(givenName))
            {
                ht[GIVEN_NAME] = givenName;
            }
        }

        public bool isSetGivenName()
        {
            return ht.ContainsKey(GIVEN_NAME);
        }

        // --- family name ---
        public string getFamilyName()
        {
            return (string)ht[FAMILY_NAME];
        }

        public void setFamilyName(string familyName)
        {
            if (!string.IsNullOrEmpty(familyName))
            {
                ht[FAMILY_NAME] = familyName;
            }
        }

        public bool isSetFamilyName()
        {
            return ht.ContainsKey(FAMILY_NAME);
        }

        // --- contact ---
        public string getContact()
        {
            return (string)ht[CONTACT];
        }

        public void setContact(string contact)
        {
            if (!string.IsNullOrEmpty(contact))
            {
                ht[CONTACT] = contact;
            }
        }

        public bool isSetContact()
        {
            return ht.ContainsKey(CONTACT);
        }

        // --- created date ---
        public string getCreatedDate()
        {
            return (string)ht[CREATED_DATE];
        }

        public void setCreatedDate(string date)
        {
            if (!string.IsNullOrEmpty(date))
            {
                ht[CREATED_DATE] = date;
            }
        }

        public bool isSetCreatedDate()
        {
            return ht.ContainsKey(CREATED_DATE);
        }

        // --- modified date ---
        public string getModifiedDate()
        {
            return (string)ht[MODIFIED_DATE];
        }

        public void setModifiedDate(string date)
        {
            if (!string.IsNullOrEmpty(date))
            {
                ht[MODIFIED_DATE] = date;
            }
        }

        public bool isSetModifiedDate()
        {
            return ht.ContainsKey(MODIFIED_DATE);
        }

        // --- type ---
        public ModelType getType()
        {
            return (ModelType)ht[TYPE];
        }

        public void setType(ModelType type)
        {
            ht[TYPE] = type;
        }

        public bool isSetType()
        {
            return ht.ContainsKey(TYPE);
        }

        // --- rights ---
        public string getRights()
        {
            return (string)ht[RIGHTS];
        }

        public void setRights(string rights)
        {
            ht[RIGHTS] = rights;
        }

        public bool isSetRights()
        {
            return ht.ContainsKey(RIGHTS);
        }

        // --- reference link ---
        public string getReferenceLink()
        {
            return (string)ht[REFERENCE_LINK];
        }

        public void setReferenceLink(String link)
        {
            if (!string.IsNullOrEmpty(link))
            {
                ht[REFERENCE_LINK] = link;
            }
        }

        public bool isSetReferenceLink()
        {
            return ht.ContainsKey(REFERENCE_LINK);
        }
    }

    class MetadataAnnotation
    {

        private const string METADATA_NS = "pmf";
        private const string METADATA_TAG = "metadata";

        private const string CREATOR_NS = "dc";
        private const string CREATOR_TAG = "creator";

        private const string CREATED_NS = "dcterms";
        private const string CREATED_TAG = "created";

        private const string MODIFIED_NS = "dcterms";
        private const string MODIFIED_TAG = "modified";

        private const string TYPE_NS = "dc";
        private const string TYPE_TAG = "type";

        private const string RIGHTS_NS = "dc";
        private const string RIGHTS_TAG = "rights";

        private const string REFERENCE_NS = "dc";
        private const string REFERENCE_TAG = "source";

        Metadata metadata;
        XMLNode annotation;

        public MetadataAnnotation(Metadata metadata)
        {
            XMLTriple pmfTriple = new XMLTriple(METADATA_TAG, "", METADATA_NS);
            annotation = new XMLNode(pmfTriple);

            // Builds creator node
            if (metadata.isSetGivenName() || metadata.isSetFamilyName() || metadata.isSetContact())
            {
                string givenName = metadata.isSetGivenName() ? metadata.getGivenName() : "";
                string familyName = metadata.isSetFamilyName() ? metadata.getFamilyName() : "";
                string contact = metadata.isSetContact() ? metadata.getContact() : "";

                string creator = givenName = "." + familyName + "." + contact;
                XMLNode node = new XMLNode(new XMLTriple(CREATOR_TAG, null, CREATOR_NS));
                node.addChild(new XMLNode(creator));
                annotation.addChild(node);
            }

            // Builds created date node
            if (metadata.isSetCreatedDate())
            {
                XMLNode node = new XMLNode(new XMLTriple(CREATED_TAG, "", CREATED_NS));
                node.addChild(new XMLNode(metadata.getCreatedDate()));
                annotation.addChild(node);
            }

            // Builds modified date node
            if (metadata.isSetModifiedDate())
            {
                XMLNode node = new XMLNode(new XMLTriple(MODIFIED_TAG, "", MODIFIED_NS));
                node.addChild(new XMLNode(metadata.getModifiedDate()));
                annotation.addChild(node);
            }

            // Builds type node
            if (metadata.isSetType())
            {
                XMLNode node = new XMLNode(new XMLTriple(TYPE_TAG, "", TYPE_NS));
                node.addChild(new XMLNode(metadata.getType().ToString()));
                annotation.addChild(node);
            }

            // Builds rights node
            if (metadata.isSetRights())
            {
                XMLNode node = new XMLNode(new XMLTriple(RIGHTS_TAG, "", RIGHTS_NS));
                node.addChild(new XMLNode(metadata.getRights()));
                annotation.addChild(node);
            }

            // Builds reference node
            if (metadata.isSetReferenceLink())
            {
                XMLNode node = new XMLNode(new XMLTriple(REFERENCE_TAG, "", REFERENCE_NS));
                node.addChild(new XMLNode(metadata.getReferenceLink()));
                annotation.addChild(node);
            }

            // Copies metadata
            this.metadata = metadata;
        }

        public MetadataAnnotation(XMLNode node)
        {
            annotation = node;

            if (node.hasChild(CREATOR_TAG))
            {
                XMLNode creatorNode = node.getChild(CREATOR_TAG);
                string chars = creatorNode.getChild(0).getCharacters();
                string[] tempStrings = chars.Split('.');

                if (!string.IsNullOrEmpty(tempStrings[0]))
                {
                    metadata.setGivenName(tempStrings[0]);
                }
                if (!string.IsNullOrEmpty(tempStrings[1]))
                {
                    metadata.setFamilyName(tempStrings[1]);
                }
                if (!string.IsNullOrEmpty(tempStrings[2]))
                {
                    metadata.setContact(tempStrings[2]);
                }
            }

            if (node.hasChild(CREATED_TAG))
            {
                XMLNode createdNode = node.getChild(CREATED_TAG);
                metadata.setCreatedDate(createdNode.getChild(0).getCharacters());
            }

            if (node.hasChild(MODIFIED_TAG))
            {
                XMLNode modifiedNode = node.getChild(MODIFIED_TAG);
                metadata.setModifiedDate(modifiedNode.getChild(0).getCharacters());
            }

            // type node
            if (node.hasChild(TYPE_TAG))
            {
                XMLNode typeNode = node.getChild(TYPE_TAG);

                Type enumType = Type.GetType("pmfml-cs.ModelType");
                ModelType type = (ModelType)Enum.Parse(enumType, typeNode.getChild(0).getCharacters());
                metadata.setType(type);
            }

            // rights node
            if (node.hasChild(RIGHTS_TAG))
            {
                XMLNode rightsNode = node.getChild(RIGHTS_TAG);
                metadata.setRights(rightsNode.getChild(0).getCharacters());
            }

            // reference node
            if (node.hasChild(REFERENCE_TAG))
            {
                XMLNode referenceNode = node.getChild(REFERENCE_TAG);
                metadata.setReferenceLink(referenceNode.getChild(0).getCharacters());
            }
        }

        public Metadata getMetadata()
        {
            return metadata;
        }

        public XMLNode getAnnotation()
        {
            return annotation;
        }
    }

    /// <summary>
    /// Primary model annotation. Holds model id, model title, uncertainties,
    /// references and condId.
    /// </summary>
    public class Model1Annotation
    {
        private const string METADATA_NS = "pmf";
        private const string METADATA_TAG = "metadata";

        Uncertainties uncertainties;
        List<Reference> refs;
        int condId;
        XMLNode annotation;

        /// <summary>
        /// Gets fields from existing primary model annotation
        /// </summary>
        public Model1Annotation(XMLNode annotation)
        {
            this.annotation = annotation;

            // Gets condId
            condId = new CondIdNode(annotation.getChild(CondIdNode.TAG)).getCondId();

            // Gets model quality annotation
            if (annotation.hasChild(UncertaintyNode.TAG))
            {
                XMLNode modelQualityNode = annotation.getChild(UncertaintyNode.TAG);
                uncertainties = new UncertaintyNode(modelQualityNode).getMeasures();
            }

            // Gets references
            refs = new List<Reference>();
            for (int i = 0; i < annotation.getNumChildren(); i++)
            {
                XMLNode currentNode = annotation.getChild(i);
                if (currentNode.getName().Equals(ReferenceSBMLNode.TAG))
                {
                    refs.Add(new ReferenceSBMLNode(currentNode).toReference());
                }
            }
        }

        public Model1Annotation(Uncertainties uncertainties, List<Reference> references, int condId)
        {
            // Builds metadata node
            XMLTriple triple = new XMLTriple(METADATA_TAG, "", METADATA_NS);
            annotation = new XMLNode(triple);

            // Build uncertainties node
            annotation.addChild(new UncertaintyNode(uncertainties).getNode());

            // Builds references nodes
            foreach (Reference reference in references)
            {
                annotation.addChild(new ReferenceSBMLNode(reference).getNode());
            }

            // Builds condID node
            annotation.addChild(new CondIdNode(condId).getNode());

            // Saves fields
            this.uncertainties = uncertainties;
            this.refs = references;
            this.condId = condId;
        }

        public Uncertainties getUncertainties()
        {
            return uncertainties;
        }

        public List<Reference> getReferences()
        {
            return refs;
        }

        public int getCondId()
        {
            return condId;
        }

        public XMLNode getAnnotation()
        {
            return annotation;
        }
    }

    /// <summary>
    /// Secondary model annotation. Holds global model ID, references and
    /// uncertainty measures.
    /// </summary>
    public class Model2Annotation
    {

        private const string METADATA_NS = "pmf";
        private const string METADATA_TAG = "metadata";

        List<Reference> references;
        int globalModelID;
        Uncertainties uncertainties;
        XMLNode annotation;

        /// <summary>
        /// Gets global model id, uncertainties and literature items of the
        /// model.
        /// </summary>
        public Model2Annotation(XMLNode annotation)
        {
            this.annotation = annotation;

            // Gets global model ID
            globalModelID = new GlobalModelIdNode(
                annotation.getChild(GlobalModelIdNode.TAG)).getGlobalModelId();

            // Gets model quality annotation
            if (annotation.hasChild(UncertaintyNode.TAG))
            {
                XMLNode qualityNode = annotation.getChild(UncertaintyNode.TAG);
                uncertainties = new UncertaintyNode(qualityNode).getMeasures();
            }

            // Gets references
            references = new List<Reference>();
            for (int i = 0; i < annotation.getNumChildren(); i++)
            {
                XMLNode currentNode = annotation.getChild(i);
                if (currentNode.getName().Equals(ReferenceSBMLNode.TAG))
                {
                    ReferenceSBMLNode refNode = new ReferenceSBMLNode(currentNode);
                    references.Add(refNode.toReference());
                }
            }
        }

        /// <summary>
        /// Builds new coefficient annotation for global model id, uncertainties
        /// and references.
        /// </summary>
        public Model2Annotation(int globalModelID, Uncertainties uncertainties,
            List<Reference> references)
        {
            annotation = new XMLNode(new XMLTriple(METADATA_TAG, "", METADATA_NS));

            // Builds GlobalModelIdNode
            annotation.addChild(new GlobalModelIdNode(globalModelID).getNode());

            // Builds UncertaintyNode
            annotation.addChild(new UncertaintyNode(uncertainties).getNode());

            // Builds reference nodes
            foreach (Reference reference in references)
            {
                annotation.addChild(new ReferenceSBMLNode(reference).getNode());
            }

            // Saves fields
            this.globalModelID = globalModelID;
            this.references = references;
            this.uncertainties = uncertainties;
        }

        public int getGlobalModelID() { return globalModelID; }
        public List<Reference> getReferences() { return references; }
        public Uncertainties getUncertainties() { return uncertainties; }
        public XMLNode getAnnotation() { return annotation; }
    }

    /// <summary>Model rule.</summary>
    /// <list type="bullet">
    /// <item><description>Formula (mandatory)</description></item>
    /// <item><description>Variable (mandatory)</description></item>
    /// <item><description>Formula name (mandatory)</description></item>
    /// <item><description>Model class (mandatory)</description></item>
    /// <item><description>PmmLab id (mandatory)</description></item>
    /// <item><description>References (optional)</description></item>
    /// </list>
    public class ModelRule
    {
        private AssignmentRule rule;
        public string formulaName { get; set; }
        public ModelClass modelClass { get; set; }
        public int pmmLabId { get; set; }
        public List<Reference> references { get; set; }

        public ModelRule(string variable, string formula, string formulaName,
            ModelClass modelClass, int pmmlabId, List<Reference> references)
        {
            this.formulaName = formulaName;
            this.modelClass = modelClass;
            this.pmmLabId = pmmLabId;
            this.references = references;

            rule = new AssignmentRule(3, 1);
            rule.setMath(libsbml.parseFormula(formula));
            rule.setVariable(variable);
            ModelRuleAnnotation annot = new ModelRuleAnnotation(formulaName, modelClass, pmmLabId, references);
            rule.setAnnotation(annot.getAnnotation());
        }

        public ModelRule(AssignmentRule rule)
        {
            this.rule = rule;
            if (rule.isSetAnnotation())
            {
                ModelRuleAnnotation annot = new ModelRuleAnnotation(rule.getAnnotation());
                pmmLabId = annot.pmmLabId;
                formulaName = annot.formulaName;
                modelClass = annot.modelClass;
                references = annot.references;
            }
        }

        public AssignmentRule getRule() { return rule; }
        public string getFormula() { return libsbml.formulaToString(rule.getMath()); }
        public string getVariable() { return rule.getVariable(); }

        public bool isSetReferences() { return references.Count > 0; }
    }

    class ModelRuleAnnotation
    {
        public string formulaName { get; }
        public ModelClass modelClass { get; }
        public List<Reference> references { get; }
        public int pmmLabId { get; }
        XMLNode annotation;

        private const string FORMULA_TAG = "formulaName";
        private const string SUBJECT_TAG = "subject";
        private const string REFERENCE_TAG = "reference";
        private const string PMMLAB_ID = "pmmlabId";

        public ModelRuleAnnotation(XMLNode annotation)
        {
            // Gets formula node
            XMLNode nameNode = annotation.getChild(FORMULA_TAG);
            formulaName = nameNode.getChild(0).getCharacters();

            // Gets formula subject
            if (annotation.hasChild(SUBJECT_TAG))
            {
                // TODO: set annotation from string in node ...
            }
            else
            {
                modelClass = ModelClass.UNKNOW;
            }

            // Gets PmmLab ID
            if (annotation.hasChild(PMMLAB_ID))
            {
                pmmLabId = int.Parse(annotation.getChild(0).getCharacters());
            }
            else
            {
                pmmLabId = -1;
            }

            // Gets references
            references = new List<Reference>();
            for (int i = 0; i < annotation.getNumChildren(); i++)
            {
                XMLNode currentNode = annotation.getChild(i);
                if (currentNode.getName().Equals(ReferenceSBMLNode.TAG))
                {
                    references.Add(new ReferenceSBMLNode(currentNode).toReference());
                }
            }
        }

        public ModelRuleAnnotation(string formulaName, ModelClass modelClass, int pmmlabId,
            List<Reference> references)
        {
            // Builds metadata node
            annotation = new XMLNode(new XMLTriple("metadata", "", "pmf"));

            // Creates annotation for formula name
            XMLNode nameNode = new XMLNode(new XMLTriple(FORMULA_TAG, "", "pmmlab"));
            nameNode.addChild(new XMLNode(formulaName));
            annotation.addChild(nameNode);

            // Creates annotation for modelClass
            XMLNode modelClassNode = new XMLNode(new XMLTriple(SUBJECT_TAG, "", "pmmlab"));
            modelClassNode.addChild(new XMLNode(Extensions.modelClassName(modelClass)));
            annotation.addChild(modelClassNode);

            // Creates annotation for pmmlabId
            XMLNode idNode = new XMLNode(new XMLTriple(PMMLAB_ID, "", "pmmlab"));
            idNode.addChild(new XMLNode(pmmlabId.ToString()));

            // Builds reference nodes
            foreach (Reference reference in references)
            {
                annotation.addChild(new ReferenceSBMLNode(reference).getNode());
            }

            // Saves formulaName, subject and model literature
            this.formulaName = formulaName;
            this.modelClass = modelClass;
            this.pmmLabId = pmmlabId;
            this.references = references;
        }

        public XMLNode getAnnotation() { return annotation; }
    }

    public class ModelVariable
    {
        public string name { get; }
        public double value { get; }

        public ModelVariable(string name, double value)
        {
            this.name = name;
            this.value = value;
        }

        public override string ToString()
        {
            if (double.IsNaN(value))
                return "ModelVariable [name=" + name + "]";
            return "ModelVariable [name=" + name + ", value=" + value + "]";
        }
    }

    /// <summary>
    /// Coefficient that extends the SBML Parameter with more data: P, error,
    /// correlations and a description tag.
    /// </summary>
    public interface PMFCoefficient
    {

        Parameter getParameter();

        string getId();
        void setId(string id);

        double getValue();
        void setValue(double value);

        string getUnit();
        void setUnit(string unit);

        double getP();
        void setP(double p);
        bool isSetP();

        double getError();
        void setError(double error);
        bool isSetError();

        double getT();
        void setT(double t);
        bool isSetT();

        List<Correlation> getCorrelations();
        void setCorrelations(List<Correlation> correlations);
        bool isSetCorrelations();

        string getDescription();
        void setDescription(string description);
        bool isSetDescription();

        bool isStart();
        void setIsStart(bool isStart);
    }

    public class PMFCoefficientImpl : PMFCoefficient
    {
        private const int LEVEL = 3;
        private const int VERSION = 1;

        private Parameter param;
        private double p;
        private double error;
        private double t;
        private List<Correlation> correlations;
        private string desc;
        private bool _isStart;

        private static string P_NS = "pmmlab";
        private static string P_TAG = "P";

        private static string ERROR_NS = "pmmlab";
        private static string ERROR_TAG = "error";

        private static string T_NS = "pmmlab";
        private static string T_TAG = "t";

        private static string CORRELATION_NS = "pmmlab";
        private static string CORRELATION_TAG = "correlation";

        private static string ATTRIBUTE_NAME = "origname";
        private static string ATTRIBUTE_VALUE = "value";

        private static string DESC_NS = "pmmlab";
        private static string DESC_TAG = "description";

        private static string METADATA_NS = "pmf";
        private static string METADATA_TAG = "pmmlab";

        private static string ISSTART_NS = "pmmlab";
        private static string ISSTART_TAG = "isStart";

        public PMFCoefficientImpl(Parameter parameter)
        {

            param = parameter;

            // Parses annotation
            XMLNode annotation = parameter.getAnnotation();

            // Gets P
            if (annotation.hasChild(P_TAG))
            {
                XMLNode pNode = annotation.getChild(P_TAG);
                p = double.Parse(pNode.getChild(0).getCharacters());
            }

            // Gets error
            if (annotation.hasChild(ERROR_TAG))
            {
                XMLNode errorNode = annotation.getChild(ERROR_TAG);
                error = double.Parse(errorNode.getChild(0).getCharacters());
            }

            // Gets t
            if (annotation.hasChild(T_TAG))
            {
                XMLNode tNode = annotation.getChild(T_TAG);
                t = double.Parse(tNode.getChild(0).getCharacters());
            }

            // Gets correlations
            correlations = new List<Correlation>();
            for (int i = 0; i < annotation.getNumChildren(); i++)
            {
                XMLNode currentNode = annotation.getChild(i);
                if (currentNode.getName().Equals(CORRELATION_TAG))
                {
                    XMLAttributes attrs = currentNode.getAttributes();
                    string corrName = attrs.getValue(ATTRIBUTE_NAME);

                    if (attrs.hasAttribute(ATTRIBUTE_VALUE))
                    {
                        string valueAsString = attrs.getValue(ATTRIBUTE_VALUE);
                        double corrValue = double.Parse(valueAsString);
                        correlations.Add(new Correlation(corrName, corrValue));
                    }
                    else
                    {
                        correlations.Add(new Correlation(corrName));
                    }
                }
            }

            // Gets description
            if (annotation.hasChild(DESC_TAG))
            {
                XMLNode descNode = annotation.getChild(DESC_TAG);
                desc = descNode.getChild(0).getCharacters();
            }

            // Gets isStart
            if (annotation.hasChild(ISSTART_TAG))
            {
                XMLNode isStartNode = annotation.getChild(ISSTART_TAG);
                _isStart = bool.Parse(isStartNode.getChild(0).getCharacters());
            }
            else
            {
                _isStart = false;
            }
        }

        public PMFCoefficientImpl(string id, double value, string unit, double p,
            double error, double t, List<Correlation> correlations, string desc,
            bool isStart)
        {
            param = new Parameter(LEVEL, VERSION);
            param.setId(id);
            param.setValue(value);
            param.setUnits(unit);
            param.setConstant(true);

            XMLNode annotation = new XMLNode(new XMLTriple(METADATA_TAG, "", METADATA_NS));

            // Creates P annotation
            if (!double.IsNaN(p))
            {
                XMLNode pNode = new XMLNode(new XMLTriple(P_TAG, "", P_NS));
                pNode.addChild(new XMLNode(p.ToString()));
                annotation.addChild(pNode);
            }

            // Creates error annotation
            if (!double.IsNaN(error))
            {
                XMLNode errorNode = new XMLNode(new XMLTriple(ERROR_TAG, "", ERROR_NS));
                errorNode.addChild(new XMLNode(errorNode.ToString()));
                annotation.addChild(errorNode);
            }

            // Creates t annotation
            if (!double.IsNaN(t))
            {
                XMLNode tNode = new XMLNode(new XMLTriple(T_TAG, "", T_NS));
                tNode.addChild(new XMLNode(tNode.ToString()));
                annotation.addChild(tNode);
            }

            // Creates Correlation annotations
            if (correlations != null)
            {
                foreach (Correlation correlation in correlations)
                {
                    XMLAttributes attrs = new XMLAttributes();
                    attrs.add(ATTRIBUTE_NAME, correlation.getName());
                    if (correlation.isSetValue())
                    {
                        attrs.add(ATTRIBUTE_VALUE, correlation.getValue().ToString());
                    }

                    XMLTriple triple = new XMLTriple(CORRELATION_TAG, "", CORRELATION_NS);
                    annotation.addChild(new XMLNode(triple, attrs));
                }
            }

            // Creates annotation for description
            if (!string.IsNullOrEmpty(desc))
            {
                XMLNode descNode = new XMLNode(new XMLTriple(DESC_TAG, "", DESC_NS));
                descNode.addChild(new XMLNode(desc));
                annotation.addChild(descNode);
            }

            // Creates annotation for isStart
            XMLNode isStartNode = new XMLNode(new XMLTriple(ISSTART_TAG, "", ISSTART_NS));
            isStartNode.addChild(new XMLNode(isStart.ToString()));
            annotation.addChild(isStartNode);

            param.setAnnotation(annotation);

            this.p = p;
            this.error = error;
            this.t = t;
            this.correlations = correlations;
            this.desc = desc;
            _isStart = isStart;
        }

        public PMFCoefficientImpl(string id, double value, string unit) :
            this(id, value, unit, double.NaN, double.NaN, double.NaN, null, "", false)
        {
        }

        // parameter
        public Parameter getParameter() { return param; }

        // --- id ---
        public string getId() { return param.getId(); }
        public void setId(string id) { param.setId(id); }

        // --- value ---
        public double getValue() { return param.getValue(); }
        public void setValue(double value) { param.setValue(value); }

        // --- unit ---
        public string getUnit() { return param.getUnits(); }
        public void setUnit(string unit) { param.setUnits(unit); }

        // --- P ---
        public double getP() { return p; }
        public void setP(double p) { this.p = p; }
        public bool isSetP() { return !double.IsNaN(p); }

        // --- error ---
        public double getError() { return error; }
        public void setError(double error) { this.error = error; }
        public bool isSetError() { return !double.IsNaN(error); }

        // --- T ---
        public double getT() { return t; }
        public void setT(double t) { this.t = t; }
        public bool isSetT() { return !double.IsNaN(t); }

        // --- correlations ---
        public List<Correlation> getCorrelations() { return correlations; }
        public void setCorrelations(List<Correlation> correlations) { this.correlations = correlations; }
        public bool isSetCorrelations() { return correlations != null; }

        // --- description ---
        public string getDescription() { return desc; }
        public void setDescription(string desc) { this.desc = desc; }
        public bool isSetDescription() { return !string.IsNullOrEmpty(desc); }

        // --- isStart ---
        public bool isStart() { return _isStart; }
        public void setIsStart(bool isStart) { _isStart = isStart; }
    }

    public interface PMFCompartment
    {
        string getId();
        void setId(string id);

        string getName();
        void setName(string name);

        string getPmfCode();
        void setPmfCode(string code);
        bool isSetPmfCode();

        string getDetail();
        void setDetail(string detail);
        bool isSetDetail();

        List<ModelVariable> getModelVariables();
        void setModelVariables(List<ModelVariable> modelVariables);
        bool isSetModelVariables();
    }

    public class PMFCompartmentImpl : PMFCompartment
    {
        private const int LEVEL = 3;
        private const int VERSION = 1;

        private const string CODE_TAG = "source";
        private const string CODE_NS = "dc";

        private const string DETAIL_TAG = "detail";
        private const string DETAIL_NS = "pmmlab";

        private const string VAR_TAG = "environment";
        private const string VAR_NS = "pmmlab";

        private const string ATTRIBUTE_NAME = "name";
        private const string ATTRIBUTE_VALUE = "value";

        private const string METADATA_NS = "pmf";
        private const string METADATA_TAG = "metadata";

        private Compartment compartment;
        private string pmfCode;
        private string detail;
        private List<ModelVariable> modelVariables;

        public PMFCompartmentImpl(Compartment compartment)
        {
            this.compartment = compartment;

            if (compartment.isSetAnnotation())
            {
                XMLNode annot = compartment.getAnnotation();

                // Gets PMF code
                if (annot.hasChild(CODE_TAG))
                {
                    XMLNode codeNode = annot.getChild(CODE_TAG);
                    pmfCode = codeNode.getChild(0).getCharacters();
                }

                // Gets details
                if (annot.hasChild(DETAIL_TAG))
                {
                    XMLNode detailsNode = annot.getChild(DETAIL_TAG);
                    detail = detailsNode.getChild(0).getCharacters();
                }

                // Gets model variables
                modelVariables = new List<ModelVariable>();
                for (int i = 0; i < annot.getNumChildren(); i++)
                {
                    XMLNode currentNode = annot.getChild(i);
                    if (currentNode.getName().Equals(VAR_TAG))
                    {
                        XMLAttributes attrs = currentNode.getAttributes();
                        string name = attrs.getValue(ATTRIBUTE_NAME);
                        double value = attrs.hasAttribute(ATTRIBUTE_VALUE)
                            ? double.Parse(attrs.getValue(ATTRIBUTE_VALUE)) : double.NaN;
                        ModelVariable mv = new ModelVariable(name, value);
                        modelVariables.Add(mv);
                    }
                }
            }
        }

        public PMFCompartmentImpl(string id, string name, string pmfCode, string detail,
            List<ModelVariable> modelVariables)
        {
            compartment = new Compartment(LEVEL, VERSION);
            compartment.setId(id);
            compartment.setName(name);
            compartment.setConstant(true);
            this.pmfCode = pmfCode;
            this.detail = detail;
            this.modelVariables = modelVariables;

            // Builds metadata node
            XMLNode annot = new XMLNode(new XMLTriple(METADATA_TAG, "", METADATA_NS));

            // Creates annotation for the PMF code
            if (!string.IsNullOrEmpty(pmfCode))
            {
                XMLNode codeNode = new XMLNode(new XMLTriple(CODE_TAG, "", CODE_NS));
                codeNode.addChild(new XMLNode(pmfCode));
                annot.addChild(codeNode);
            }

            // Creates annotation for the matrix details
            if (!string.IsNullOrEmpty(detail))
            {
                XMLNode detailsNode = new XMLNode(new XMLTriple(DETAIL_TAG, "", DETAIL_NS));
                detailsNode.addChild(new XMLNode(detail));
                annot.addChild(detailsNode);
            }

            // Creates annotation for model variables (Temperature, pH, aW)
            if (modelVariables != null)
            {
                XMLTriple varTriple = new XMLTriple(VAR_TAG, "", VAR_NS);
                foreach (ModelVariable mv in modelVariables)
                {
                    XMLAttributes attrs = new XMLAttributes();
                    attrs.add(ATTRIBUTE_NAME, mv.name);
                    if (!double.IsNaN(mv.value))
                    {
                        attrs.add(ATTRIBUTE_VALUE, mv.value.ToString());
                    }

                    annot.addChild(new XMLNode(varTriple, attrs));
                }
            }

            compartment.setAnnotation(annot);
        }

        public PMFCompartmentImpl(string id, string name)
        {
            compartment = new Compartment(LEVEL, VERSION);
            compartment.setId(id);
            compartment.setName(name);
            compartment.setConstant(true);
        }

        // --- id ---
        public string getId() { return compartment.getId(); }
        public void setId(string id) { compartment.setId(id); }

        // -- name ---
        public string getName() { return compartment.getName(); }
        public void setName(string name) { compartment.setName(name); }

        // --- pmf code ---
        public string getPmfCode() { return pmfCode; }
        public void setPmfCode(string code) { pmfCode = code; }
        public bool isSetPmfCode() { return !string.IsNullOrEmpty(pmfCode); }

        // --- detail ---
        public string getDetail() { return detail; }
        public void setDetail(string detail) { this.detail = detail; }
        public bool isSetDetail() { return !string.IsNullOrEmpty(detail); }

        // --- model variables ---
        public List<ModelVariable> getModelVariables() { return modelVariables; }
        public void setModelVariables(List<ModelVariable> modelVariables) { this.modelVariables = modelVariables; }
        public bool isSetModelVariables() { return modelVariables != null; }

        // getCompartment
        public Compartment getCompartment() { return compartment; }
    }

    public interface PMFSpecies
    {
        Species getSpecies();

        string getCompartment();
        void setCompartment(string compartment);

        string getId();
        void setId(string id);

        string getName();
        void setName(string name);

        string getUnits();
        void setUnits(string units);

        string getCombaseCode();
        void setCombaseCode(string code);
        bool isSetCombaseCode();

        string getDetail();
        void setDetail(string detail);
        bool isSetDetail();

        string getDescription();
        void setDescription(string description);
        bool isSetDescription();
    }

    public class PMFSpeciesImpl : PMFSpecies
    {
        public static bool BOUNDARY_CONDITION = true;
        public static bool CONSTANT = false;
        public static bool ONLY_SUBSTANCE_UNITS = false;

        const int LEVEL = 3;
        const int VERSION = 1;

        const string SOURCE_NS = "dc";
        const string SOURCE_TAG = "source";

        const string DETAIL_NS = "pmmlab";
        const string DETAIL_TAG = "detail";

        const string DESC_NS = "pmmlab";
        const string DESC_TAG = "desc";

        const string METADATA_NS = "pmf";
        const string METADATA_TAG = "metadata";

        private Species species;
        private string combaseCode;
        private string detail;
        private string description;

        public PMFSpeciesImpl(Species species)
        {
            this.species = species;

            if (species.isSetAnnotation())
            {
                XMLNode annot = species.getAnnotation();

                // Gets CAS number
                if (annot.hasChild(SOURCE_TAG))
                {
                    XMLNode sourceNode = annot.getChild(SOURCE_TAG);
                    string wholeReference = sourceNode.getChild(0).getCharacters();
                    combaseCode = wholeReference.Substring(wholeReference.LastIndexOf('/') + 1);
                }

                // Gets description
                if (annot.hasChild(DETAIL_TAG))
                {
                    XMLNode detailNode = annot.getChild(DETAIL_TAG);
                    detail = detailNode.getChild(0).getCharacters();
                }

                // Gets dep description
                if (annot.hasChild(DESC_TAG))
                {
                    XMLNode descNode = annot.getChild(DESC_TAG);
                    description = descNode.getChild(0).getCharacters();
                }
            }
        }

        public PMFSpeciesImpl(string compartment, string id, string name,
            string substanceUnits, string combaseCode, string detail,
            string description)
        {
            species = new Species(LEVEL, VERSION);
            species.setId(id);
            species.setName(name);
            species.setSubstanceUnits(substanceUnits);
            species.setBoundaryCondition(BOUNDARY_CONDITION);
            species.setConstant(CONSTANT);
            species.setHasOnlySubstanceUnits(ONLY_SUBSTANCE_UNITS);

            if (!string.IsNullOrEmpty(combaseCode) ||
                !string.IsNullOrEmpty(detail) ||
                !string.IsNullOrEmpty(description))
            {
                XMLTriple triple = new XMLTriple(METADATA_TAG, "", METADATA_NS);
                XMLNode annot = new XMLNode(triple);

                // Builds reference tag
                if (!string.IsNullOrEmpty(combaseCode))
                {
                    XMLTriple refTriple = new XMLTriple(SOURCE_TAG, "", SOURCE_NS);
                    XMLNode refNode = new XMLNode(refTriple);
                    refNode.addChild(new XMLNode("http://identifiers.org/ncim/" + combaseCode));
                    annot.addChild(refNode);

                    this.combaseCode = combaseCode;
                }

                // Builds detail tag
                if (!string.IsNullOrEmpty(detail))
                {
                    XMLTriple detailTriple = new XMLTriple(DETAIL_TAG, "", DETAIL_NS);
                    XMLNode detailNode = new XMLNode(detailTriple);
                    annot.addChild(new XMLNode(detailNode));

                    this.detail = detail;
                }

                // Builds dep description tag
                if (!string.IsNullOrEmpty(description))
                {
                    XMLTriple descTriple = new XMLTriple(DESC_TAG, "", DESC_NS);
                    XMLNode descNode = new XMLNode(descTriple);
                    annot.addChild(descNode);

                    this.description = description;
                }
            }
        }

        // constructor with compartment, id, name and substanceUnits
        public PMFSpeciesImpl(string compartment, string id, string name, string substanceUnits)
        : this(compartment, id, name, substanceUnits, null, null, null) { }

        // --- species ---
        public Species getSpecies() { return species; }

        // --- compartment ---
        public string getCompartment() { return species.getCompartment(); }
        public void setCompartment(string compartment) { species.setCompartment(compartment); }

        // --- id ---
        public string getId() { return species.getId(); }
        public void setId(string id) { species.setId(id); }

        // --- name ---
        public string getName() { return species.getName(); }
        public void setName(string name) { species.setName(name); }

        // --- units ---
        public string getUnits() { return species.getUnits(); }
        public void setUnits(string units) { species.setUnits(units); }

        // --- combase code ---
        public string getCombaseCode() { return combaseCode; }
        public void setCombaseCode(string combaseCode) { this.combaseCode = combaseCode; }
        public bool isSetCombaseCode() { return !string.IsNullOrEmpty(combaseCode); }

        // --- detail ---
        public string getDetail() { return detail; }
        public void setDetail(string detail) { this.detail = detail; }
        public bool isSetDetail() { return !string.IsNullOrEmpty(detail); }

        // --- description ---
        public string getDescription() { return description; }
        public void setDescription(string description) { this.description = description; }
        public bool isSetDescription() { return !string.IsNullOrEmpty(description); }
    }

    public class PMFUnit
    {
        private const int LEVEL = 3;
        private const int VERSION = 1;

        public const double DEFAULT_MULTIPLIER = 1.0;
        public const int DEFAULT_SCALE = 0;
        public const double DEFAULT_EXPONENT = 1.0;

        Unit unit;

        public PMFUnit(double multiplier, int scale, int kind, double exponent)
        {
            unit = new Unit(LEVEL, VERSION);
            unit.setMultiplier(multiplier);
            unit.setScale(scale);
            unit.setKind(kind);
            unit.setExponent(exponent);
        }

        public double getMultiplier() { return unit.getMultiplier(); }
        public void setMultipliert(double multiplier) { unit.setMultiplier(multiplier); }

        public int getScale() { return unit.getScale(); }
        public void setScale(int scale) { unit.setScale(scale); }

        public int getKind() { return unit.getKind(); }
        public void setKind(int kind) { unit.setKind(kind); }

        public double getExponent() { return unit.getExponent(); }
        public void setExponent(double exponent) { unit.setExponent(exponent); }
    }

    public class PMFUnitDefinition
    {
        private const int LEVEL = 3;
        private const int VERSION = 1;

        const string TRANSFORMATION = "transformation";

        public string transformationName { get; set; }
        UnitDefinition unitDefinition;

        public PMFUnitDefinition(string id, string name,
            string transformationName, List<Unit> units)
        {
            unitDefinition = new UnitDefinition(LEVEL, VERSION);

            // Adds units
            if (units != null)
            {
                foreach (Unit unit in units)
                {
                    unitDefinition.addUnit(unit);
                }
            }

            // Add transformation node
            if (!string.IsNullOrEmpty(transformationName))
            {
                // Creates transformation node
                XMLTriple transformationTriple = new XMLTriple(TRANSFORMATION, "", "pmmlab");
                XMLNode transformationNode = new XMLNode(transformationTriple);
                transformationNode.addChild(new XMLNode(transformationName));

                // Creates metadata node
                XMLTriple metadataTriple = new XMLTriple("metadata", "", "pmf");
                XMLNode metadata = new XMLNode(metadataTriple);
                metadata.addChild(transformationNode);

                // Sets annotation
                unitDefinition.setAnnotation(metadata);

                // copies transformation
                this.transformationName = transformationName;
            }
        }

        public PMFUnitDefinition(UnitDefinition unitDefinition)
        {
            this.unitDefinition = unitDefinition;
            if (unitDefinition.isSetAnnotation())
            {
                XMLNode metadata = unitDefinition.getAnnotation();
                XMLNode transformationNode = metadata.getChild(TRANSFORMATION);
                transformationName = transformationNode.getChild(0).getCharacters();
            }
        }

        public UnitDefinition getUnitDefinition() { return unitDefinition; }

        public string getId() { return unitDefinition.getId(); }
        public void setId(string id) { unitDefinition.setId(id); }

        public string getName() { return unitDefinition.getName(); }
        public void setName(string name) { unitDefinition.setName(name); }

        public List<Unit> getUnits()
        {
            ListOfUnits listOfUnits = unitDefinition.getListOfUnits();
            int numUnits = (int)listOfUnits.size();

            List<Unit> list = new List<Unit>(numUnits);
            for (int i = 0; i < numUnits; i++)
            {
                list.Add(listOfUnits.get(i));
            }

            return list;
        }

        public void setUnits(List<Unit> units)
        {
            unitDefinition.getListOfUnits().clear();
            foreach (Unit unit in units)
            {
                unitDefinition.addUnit(unit);
            }
        }

        public bool isSetTransformationName() { return !string.IsNullOrEmpty(transformationName); }
    }

    public class PrimaryModelNode
    {
        const string TAG = "primaryModel";
        const string NS = "pmmlab";

        XMLNode node;

        public PrimaryModelNode(string model)
        {
            node = new XMLNode(new XMLTriple(TAG, "", NS));
            node.addChild(new XMLNode(model));
        }

        public PrimaryModelNode(XMLNode node)
        {
            this.node = node;
        }

        public string getPrimaryModel() { return node.getChild(0).getCharacters(); }
        public XMLNode getNode() { return node; }
    }

    public interface Reference
    {

        string getAuthor();
        void setAuthor(string author);
        bool isSetAuthor();

        int getYear();
        void setYear(int year);
        bool isSetYear();

        string getTitle();
        void setTitle(string title);
        bool isSetTitle();

        string getAbstractText();
        void setAbstractText(string text);
        bool isSetAbstractText();

        string getJournal();
        void setJournal(string journal);
        bool isSetJournal();

        string getVolume();
        void setVolume(string volume);
        bool isSetVolume();

        string getIssue();
        void setIssue(string issue);
        bool isSetIssue();

        int getPage();
        void setPage(int page);
        bool isSetPage();

        int getApprovalMode();
        void setApprovalMode(int mode);
        bool isSetApprovalMode();

        string getWebsite();
        void setWebsite(string website);
        bool isSetWebsite();

        ReferenceType getType();
        void setType(ReferenceType type);
        bool isSetType();

        string getComment();
        void setComment(string comment);
        bool isSetComment();
    }

    public enum ReferenceType
    {
        Paper = 1,
        SOP = 2,
        LA = 3,
        Handbuch = 4,
        Laborbuch = 5,
        Buch = 6,
        Webseite = 7,
        Berich = 8
    };

    public class ReferenceImpl : Reference
    {
        private const string AUTHOR = "reference";
        private const string YEAR = "year";
        private const string TITLE = "abstract";
        private const string ABSTRACT_TEXT = "abstractText";
        private const string JOURNAL = "journal";
        private const string VOLUME = "volume";
        private const string ISSUE = "issue";
        private const string PAGE = "page";
        private const string APPROVAL_MODE = "approvalMode";
        private const string WEBSITE = "website";
        private const string TYPE = "type";
        private const string COMMENT = "comment";

        private Hashtable ht;

        public ReferenceImpl()
        {
            ht = new Hashtable();
        }

        // author
        string Reference.getAuthor() { return (string)ht[AUTHOR]; }
        void Reference.setAuthor(string author)
        {
            if (!string.IsNullOrEmpty(author))
                ht[AUTHOR] = author;
        }
        bool Reference.isSetAuthor() { return ht.ContainsKey(AUTHOR); }

        // year
        int Reference.getYear() { return (int)ht[YEAR]; }
        void Reference.setYear(int year) { ht[YEAR] = year; }
        bool Reference.isSetYear() { return ht.ContainsKey(YEAR); }

        // title
        string Reference.getTitle() { return (string)ht[TITLE]; }
        void Reference.setTitle(string title) { ht[TITLE] = title; }
        bool Reference.isSetTitle() { return ht.ContainsKey(TITLE); }

        // abstract text
        string Reference.getAbstractText() { return (string)ht[ABSTRACT_TEXT]; }
        void Reference.setAbstractText(string abstractText)
        {
            if (!string.IsNullOrEmpty(abstractText))
            {
                ht[ABSTRACT_TEXT] = abstractText;
            }
        }
        bool Reference.isSetAbstractText() { return ht.ContainsKey(ABSTRACT_TEXT); }

        // journal
        string Reference.getJournal() { return (string)ht[JOURNAL]; }
        void Reference.setJournal(string journal)
        {
            if (!string.IsNullOrEmpty(journal))
            {
                ht[JOURNAL] = journal;
            }
        }
        bool Reference.isSetJournal() { return ht.ContainsKey(JOURNAL); }

        // volume
        public string getVolume() { return (string)ht[VOLUME]; }
        public void setVolume(string volume)
        {
            if (!string.IsNullOrEmpty(volume))
            {
                ht[VOLUME] = volume;
            }
        }
        public bool isSetVolume() { return ht.ContainsKey(VOLUME); }

        // issue
        public string getIssue() { return (string)ht[ISSUE]; }
        public void setIssue(string issue)
        {
            if (!string.IsNullOrEmpty(issue))
            {
                ht[ISSUE] = issue;
            }
        }
        public bool isSetIssue() { return ht.ContainsKey(ISSUE); }

        // page
        public int getPage() { return (int)ht[PAGE]; }
        public void setPage(int page) { ht[PAGE] = page; }
        public bool isSetPage() { return ht.ContainsKey(PAGE); }

        // approval mode
        public int getApprovalMode() { return (int)ht[APPROVAL_MODE]; }
        public void setApprovalMode(int mode) { ht[APPROVAL_MODE] = mode; }
        public bool isSetApprovalMode() { return ht.ContainsKey(APPROVAL_MODE); }

        // website
        public string getWebsite() { return (string)ht[WEBSITE]; }
        public void setWebsite(string website)
        {
            if (!string.IsNullOrEmpty(website))
            {
                ht[WEBSITE] = website;
            }
        }
        public bool isSetWebsite() { return ht.ContainsKey(WEBSITE); }

        // type
        public ReferenceType getType() { return (ReferenceType)ht[TYPE]; }
        public void setType(ReferenceType type) { ht[TYPE] = type; }
        public bool isSetType() { return ht.ContainsKey(TYPE); }

        // comment
        public string getComment() { return (string)ht[COMMENT]; }
        public void setComment(string comment)
        {
            if (!string.IsNullOrEmpty(comment))
            {
                ht[COMMENT] = comment;
            }
        }
        public bool isSetComment() { return ht.ContainsKey(COMMENT); }
    }

    public class ReferenceSBMLNode
    {
        public const string NS = "dc";
        public const string TAG = "reference";

        private LiteratureSpecificationI spec = new RIS();

        XMLNode node;

        public ReferenceSBMLNode(Reference reference)
        {
            // Reference container
            XMLNamespaces namespaces = new XMLNamespaces();
            namespaces.add("http://foo.bar.com", "ref");
            XMLTriple refTriple = new XMLTriple(TAG, "", NS);
            XMLNode refNode = new XMLNode(refTriple, null, namespaces);

            // author node
            if (reference.isSetAuthor())
            {
                XMLTriple triple = new XMLTriple(spec.getAuthor(), "", "ref");
                XMLNode node = new XMLNode(reference.getAuthor());
                refNode.addChild(node);
            }

            // title node
            if (reference.isSetTitle())
            {
                XMLTriple triple = new XMLTriple(spec.getTitle(), "", "ref");
                XMLNode node = new XMLNode(reference.getTitle());
                refNode.addChild(node);
            }

            //  abstract node
            if (reference.isSetAbstractText())
            {
                XMLTriple triple = new XMLTriple(spec.getAbstract(), "", "ref");
                XMLNode node = new XMLNode(reference.getAbstractText());
                refNode.addChild(node);
            }

            // journal node
            if (reference.isSetJournal())
            {
                XMLTriple triple = new XMLTriple(spec.getJournal(), "", "ref");
                XMLNode node = new XMLNode(reference.getJournal());
                refNode.addChild(node);
            }

            // volume node
            if (reference.isSetVolume())
            {
                XMLTriple triple = new XMLTriple(spec.getVolume(), "", "ref");
                XMLNode node = new XMLNode(reference.getVolume());
                refNode.addChild(node);
            }

            // issue node
            if (reference.isSetIssue())
            {
                XMLTriple triple = new XMLTriple(spec.getIssue(), "", "ref");
                XMLNode node = new XMLNode(reference.getIssue());
                refNode.addChild(node);
            }

            // page
            if (reference.isSetPage())
            {
                XMLTriple triple = new XMLTriple(spec.getPage(), "", "ref");
                XMLNode node = new XMLNode(reference.getPage().ToString());
                refNode.addChild(node);
            }

            // approval mode
            if (reference.isSetApprovalMode())
            {
                XMLTriple triple = new XMLTriple(spec.getApproval(), "", "ref");
                XMLNode node = new XMLNode(reference.getApprovalMode().ToString());
                refNode.addChild(node);
            }

            // website
            if (reference.isSetWebsite())
            {
                XMLTriple triple = new XMLTriple(spec.getWebsite(), "", "ref");
                XMLNode node = new XMLNode(reference.getWebsite());
                refNode.addChild(node);
            }

            // type
            if (reference.isSetType())
            {
                XMLTriple triple = new XMLTriple(spec.getType(), "", "ref");
                XMLNode node = new XMLNode(reference.getType().ToString());
                refNode.addChild(node);
            }

            // comment
            if (reference.isSetComment())
            {
                XMLTriple triple = new XMLTriple(spec.getComment(), "", "ref");
                XMLNode node = new XMLNode(reference.getComment());
                refNode.addChild(node);
            }
        }

        public ReferenceSBMLNode(XMLNode node)
        {
            this.node = node;
        }

        public Reference toReference()
        {
            Reference reference = new ReferenceImpl();

            // author
            if (node.hasChild(spec.getAuthor()))
            {
                XMLNode authorNode = node.getChild(spec.getAuthor());
                string author = authorNode.getChild(0).getCharacters();
                reference.setAuthor(author);
            }

            // title
            if (node.hasChild(spec.getTitle()))
            {
                XMLNode titleNode = node.getChild(spec.getTitle());
                string title = titleNode.getChild(0).getCharacters();
                reference.setTitle(title);
            }

            // abstract text
            if (node.hasChild(spec.getAbstract()))
            {
                XMLNode abstractNode = node.getChild(spec.getAbstract());
                string abstractText = abstractNode.getChild(0).getCharacters();
                reference.setAbstractText(abstractText);
            }

            // year
            if (node.hasChild(spec.getYear()))
            {
                XMLNode yearNode = node.getChild(spec.getYear());
                int year = int.Parse(yearNode.getChild(0).getCharacters());
                reference.setYear(year);
            }

            // journal
            if (node.hasChild(spec.getJournal()))
            {
                XMLNode journalNode = node.getChild(spec.getJournal());
                string journal = journalNode.getChild(0).getCharacters();
                reference.setJournal(journal);
            }

            // volume
            if (node.hasChild(spec.getVolume()))
            {
                XMLNode volumeNode = node.getChild(spec.getVolume());
                string volume = volumeNode.getChild(0).getCharacters();
                reference.setVolume(volume);
            }

            // issue
            if (node.hasChild(spec.getIssue()))
            {
                XMLNode issueNode = node.getChild(spec.getIssue());
                string issue = issueNode.getChild(0).getCharacters();
                reference.setIssue(issue);
            }

            // page
            if (node.hasChild(spec.getPage()))
            {
                XMLNode pageNode = node.getChild(spec.getPage());
                int page = int.Parse(pageNode.getChild(0).getCharacters());
                reference.setPage(page);
            }

            // approval mode
            if (node.hasChild(spec.getApproval()))
            {
                XMLNode approvalNode = node.getChild(spec.getApproval());
                int approval = int.Parse(approvalNode.getChild(0).getCharacters());
                reference.setApprovalMode(approval);
            }

            // website
            if (node.hasChild(spec.getWebsite()))
            {
                XMLNode websiteNode = node.getChild(spec.getWebsite());
                string website = websiteNode.getChild(0).getCharacters();
                reference.setWebsite(website);
            }

            // type
            if (node.hasChild(spec.getType()))
            {
                XMLNode typeNode = node.getChild(spec.getType());
                string typeAsString = typeNode.getChild(0).getCharacters();
                if (typeAsString.Equals("paper"))
                {
                    reference.setType(ReferenceType.Paper);
                }
                else if (typeAsString.Equals("sop"))
                {
                    reference.setType(ReferenceType.SOP);
                }
                else if (typeAsString.Equals("la"))
                {
                    reference.setType(ReferenceType.LA);
                }
                else if (typeAsString.Equals("handbuch"))
                {
                    reference.setType(ReferenceType.Handbuch);
                }
                else if (typeAsString.Equals("laborbuch"))
                {
                    reference.setType(ReferenceType.Laborbuch);
                }
                else if (typeAsString.Equals("buch"))
                {
                    reference.setType(ReferenceType.Buch);
                }
                else if (typeAsString.Equals("berich"))
                {
                    reference.setType(ReferenceType.Berich);
                }
            }

            // comment
            if (node.hasChild(spec.getComment()))
            {
                XMLNode commentNode = node.getChild(spec.getComment());
                string comment = commentNode.getChild(0).getCharacters();
                reference.setComment(comment);
            }

            return reference;
        }

        public XMLNode getNode() { return node; }
    }

    public class RIS : LiteratureSpecificationI
    {
        public string getAuthor() { return "AU"; }
        public string getYear() { return "PY"; }
        public string getTitle() { return "TI"; }
        public string getAbstract() { return "AB"; }
        public string getJournal() { return "T2"; }
        public string getVolume() { return "VL"; }
        public string getIssue() { return "IS"; }
        public string getPage() { return "SP"; }
        public string getApproval() { return "LB"; }
        public string getWebsite() { return "UR"; }
        public string getType() { return "M3"; }
        public string getComment() { return "N1"; }
    }

    public class SBMLFactory
    {
        public static Metadata createMetadata()
        {
            return new MetadataImpl();
        }

        public static Metadata createMetadata(string givenName,
            string familyName, string contact, string createdDate,
            string modifiedDate, ModelType type, string rights,
            string referenceLink)
        {
            return new MetadataImpl(givenName, familyName, contact,
                createdDate, modifiedDate, type, rights,
                referenceLink);
        }

        public static PMFCoefficient createPMFCoefficient(Parameter parameter)
        {
            return new PMFCoefficientImpl(parameter);
        }

        public static PMFCoefficient createPMFCoefficient(string id,
            double value, string unit, double p, double error, double t,
            List<Correlation> correlations, string desc, bool isStart)
        {
            return new PMFCoefficientImpl(id, value, unit, p, error, t,
                correlations, desc, isStart);
        }

        public static PMFCompartment createPMFCompartment(Compartment compartment)
        {
            return new PMFCompartmentImpl(compartment);
        }

        public static PMFCompartment createPMFCompartment(string id, string name,
            string pmfCode, string detail, List<ModelVariable> modelVariables)
        {
            return new PMFCompartmentImpl(id, name, pmfCode, detail, modelVariables);
        }

        public static PMFCompartment createPMFCompartment(string id, string name)
        {
            return new PMFCompartmentImpl(id, name);
        }

        public static PMFSpecies createPMFSpecies(Species species)
        {
            return new PMFSpeciesImpl(species);
        }

        public static PMFSpecies createPMFSpecies(string compartment, string id,
            string name, string substanceUnits, string combaseCode,
            string detail, string description)
        {
            return new PMFSpeciesImpl(compartment, id, name, substanceUnits,
                combaseCode, detail, description);
        }

        public static PMFSpecies createPMFSpecies(string compartment,
            string id, string name, string substanceUnits)
        {
            return new PMFSpeciesImpl(compartment, id, name, substanceUnits);
        }

        /* A parametrized createReference (like in the Java library) does not
         * make sense in C# since nullables are not used and many of the
         * reference fields could be empty. Thus it is better to return an
         * empty reference and let the user populate it.
         */
        public static Reference createReference()
        {
            return new ReferenceImpl();
        }

        // Same as with createReference
        public static Uncertainties createUncertainties()
        {
            return new UncertaintiesImpl();
        }
    }

    public class SecDep
    {
        Parameter param;
        String desc;

        public SecDep(Parameter param)
        {
            // If param has annotation, process it
            if (param.isSetAnnotation())
            {
                desc = new SecDepAnnotation(param.getAnnotation()).getDesc();
            }
            // Copies parameter
            this.param = param;
        }

        public SecDep(string name, string desc, string unit)
        {
            param = new Parameter(3, 1);
            param.setId(name);
            param.setConstant(false);
            param.setValue(0.0);

            if (!string.IsNullOrEmpty(desc))
            {
                param.setAnnotation(new SecDepAnnotation(desc).getAnnotation());
            }

            if (!string.IsNullOrEmpty(unit))
            {
                param.setUnits(PMFUtil.createId(unit));
            }
        }

        public Parameter getParam()
        {
            return param;
        }

        public string getDescription()
        {
            return desc;
        }
    }

    class SecDepAnnotation
    {
        private const string METADATA_NS = "pmf";
        private const string METADATA_TAG = "metadata";

        private const string DESC_NS = "pmf";
        private const string DESC_TAG = "description";

        XMLNode annotation;
        String desc;

        public SecDepAnnotation(XMLNode annotation)
        {
            this.annotation = annotation;

            XMLNode descNode = annotation.getChild(DESC_TAG);
            desc = descNode.getChild(0).getCharacters();
        }

        public SecDepAnnotation(string desc)
        {
            // Creates descriptioni node and adds it to the annotation node
            XMLTriple descTriple = new XMLTriple(DESC_TAG, "", DESC_NS);
            XMLNode descNode = new XMLNode(descTriple);
            descNode.addChild(new XMLNode(desc));

            // Creates annotation
            XMLTriple annoTriple = new XMLTriple(METADATA_TAG, "", METADATA_NS);
            annotation = new XMLNode(annoTriple);

            // Copies description
            this.desc = desc;
        }

        public XMLNode getAnnotation() { return annotation; }
        public string getDesc() { return desc; }
    }

    public class SecIndep
    {

        Parameter param;
        string desc;

        public SecIndep(Parameter param)
        {
            if (param.isSetAnnotation())
                desc = new SecIndepAnnotation(param.getAnnotation()).getDesc();
        }

        public SecIndep(string name, string desc, string unit)
        {
            param = new Parameter(3, 1);
            param.setId(name);
            param.setConstant(false);
            param.setValue(0.0);

            if (!string.IsNullOrEmpty(desc))
                param.setAnnotation(new SecIndepAnnotation(desc).getAnnotation());

            if (string.IsNullOrEmpty(unit))
            {
                param.setUnits("dimensionless");
            }
            else
            {
                param.setUnits(PMFUtil.createId(unit));
            }
        }

        public Parameter getParam() { return param; }
        public string getDescription() { return desc; }
    }

    class SecIndepAnnotation
    {
        private const string METADATA_NS = "pmf";
        private const string METADATA_TAG = "metadata";

        private const string DESC_NS = "pmf";
        private const string DESC_TAG = "description";

        XMLNode annotation;
        string desc;

        public SecIndepAnnotation(XMLNode annotation)
        {
            this.annotation = annotation;

            XMLNode descNode = annotation.getChild(METADATA_TAG);
            desc = descNode.getChild(0).getCharacters();
        }

        public SecIndepAnnotation(string desc)
        {
            // Creates description node and adds it to the annotation node
            XMLTriple descTriple = new XMLTriple(DESC_TAG, "", DESC_NS);
            XMLNode descNode = new XMLNode(descTriple);
            descNode.addChild(new XMLNode(desc));

            // Creates annotation
            XMLTriple annoTriple = new XMLTriple(METADATA_TAG, "", METADATA_NS);
            annotation = new XMLNode(annoTriple);

            // Copies description
            this.desc = desc;
        }

        public XMLNode getAnnotation() { return annotation; }
        public string getDesc() { return desc; }
    }

    public interface Uncertainties
    {
        int getId();
        void setId(int id);
        bool isSetId();

        string getModelName();
        void setModelName(string name);
        bool isSetModelName();

        string getComment();
        void setComment(string comment);
        bool isSetComment();

        double getR2();
        void setR2(double r2);
        bool isSetR2();

        double getRMS();
        void setRMS(double rms);
        bool isSetRMS();

        double getSSE();
        void setSSE(double sse);
        bool isSetSSE();

        double getAIC();
        void setAIC(double aic);
        bool isSetAIC();

        double getBIC();
        void setBIC(double bic);
        bool isSetBIC();

        int getDOF();
        void setDOF(int dof);
        bool isSetDOF();
    }

    public class UncertaintiesImpl : Uncertainties
    {
        private const string ID = "id";
        private const string MODEL_NAME = "modelName";
        private const string COMMENT = "comment";
        private const string R2 = "r2";
        private const string RMS = "rms";
        private const string SSE = "sse";
        private const string AIC = "aic";
        private const string BIC = "bic";
        private const string DOF = "dof";

        private Hashtable ht = new Hashtable();

        public int getId() { return (int)ht[ID]; }
        public void setId(int id) { ht[ID] = id; }
        public bool isSetId() { return ht.ContainsKey(ID); }

        public string getModelName() { return (string)ht[MODEL_NAME]; }
        public void setModelName(string name) { ht[MODEL_NAME] = name; }
        public bool isSetModelName() { return ht.ContainsKey(MODEL_NAME); }

        public string getComment() { return (string)ht[COMMENT]; }
        public void setComment(string comment) { ht[COMMENT] = comment; }
        public bool isSetComment() { return ht.ContainsKey(COMMENT); }

        public double getR2() { return (double)ht[R2]; }
        public void setR2(double r2) { ht[R2] = r2; }
        public bool isSetR2() { return ht.ContainsKey(R2); }

        public double getRMS() { return (double)ht[RMS]; }
        public void setRMS(double rms) { ht[RMS] = rms; }
        public bool isSetRMS() { return ht.ContainsKey(RMS); }

        public double getSSE() { return (double)ht[SSE]; }
        public void setSSE(double sse) { ht[SSE] = sse; }
        public bool isSetSSE() { return ht.ContainsKey(SSE); }

        public double getAIC() { return (double)ht[AIC]; }
        public void setAIC(double aic) { ht[AIC] = aic; }
        public bool isSetAIC() { return ht.ContainsKey(AIC); }

        public double getBIC() { return (double)ht[BIC]; }
        public void setBIC(double bic) { ht[BIC] = bic; }
        public bool isSetBIC() { return ht.ContainsKey(BIC); }

        public int getDOF() { return (int)ht[DOF]; }
        public void setDOF(int dof) { ht[DOF] = dof; }
        public bool isSetDOF() { return ht.ContainsKey(DOF); }
    }

    public class UncertaintyNode
    {
        public const string TAG = "modelquality";
        public const string NS = "pmmlab";

        public const string ID = "id";
        public const string NAME = "name";
        public const string COMMENT = "comment";
        public const string R2 = "r2";
        public const string RMS = "rms";
        public const string SSE = "sse";
        public const string AIC = "aic";
        public const string BIC = "bic";
        public const string DOF = "dof";

        private XMLNode node;

        public UncertaintyNode(XMLNode node)
        {
            this.node = node;
        }

        public UncertaintyNode(Uncertainties uncertainties)
        {
            XMLAttributes attrs = new XMLAttributes();
            if (uncertainties.isSetId())
                attrs.add(ID, uncertainties.getId().ToString());
            if (uncertainties.isSetModelName())
                attrs.add(NAME, uncertainties.getModelName());
            if (uncertainties.isSetComment())
                attrs.add(COMMENT, uncertainties.getComment());
            if (uncertainties.isSetR2())
                attrs.add(R2, uncertainties.getR2().ToString());
            if (uncertainties.isSetRMS())
                attrs.add(RMS, uncertainties.getRMS().ToString());
            if (uncertainties.isSetSSE())
                attrs.add(SSE, uncertainties.getSSE().ToString());
            if (uncertainties.isSetAIC())
                attrs.add(AIC, uncertainties.getAIC().ToString());
            if (uncertainties.isSetBIC())
                attrs.add(BIC, uncertainties.getBIC().ToString());
            if (uncertainties.isSetDOF())
                attrs.add(DOF, uncertainties.getDOF().ToString());

            node = new XMLNode(new XMLTriple(TAG, "", NS), attrs);
        }

        public Uncertainties getMeasures()
        {
            XMLAttributes attrs = node.getAttributes();

            Uncertainties uncert = new UncertaintiesImpl();
            if (attrs.hasAttribute(ID))
            {
                string attr = attrs.getValue(ID);
                uncert.setId(int.Parse(attr));
            }

            if (attrs.hasAttribute(NAME))
            {
                string attr = attrs.getValue(NAME);
                uncert.setModelName(attr);
            }

            if (attrs.hasAttribute(COMMENT))
            {
                string attr = attrs.getValue(COMMENT);
                uncert.setComment(attr);
            }

            if (attrs.hasAttribute(R2))
            {
                string attr = attrs.getValue(R2);
                uncert.setR2(double.Parse(attr));
            }

            if (attrs.hasAttribute(RMS))
            {
                string attr = attrs.getValue(RMS);
                uncert.setRMS(double.Parse(attr));
            }

            if (attrs.hasAttribute(SSE))
            {
                string attr = attrs.getValue(SSE);
                uncert.setSSE(double.Parse(attr));
            }

            if (attrs.hasAttribute(AIC))
            {
                string attr = attrs.getValue(AIC);
                uncert.setAIC(double.Parse(attr));
            }

            if (attrs.hasAttribute(BIC))
            {
                string attr = attrs.getValue(BIC);
                uncert.setBIC(double.Parse(attr));
            }

            if (attrs.hasAttribute(DOF))
            {
                string attr = attrs.getValue(DOF);
                uncert.setDOF(int.Parse(attr));
            }

            return uncert;
        }

        public XMLNode getNode() { return node; }
    }
}
