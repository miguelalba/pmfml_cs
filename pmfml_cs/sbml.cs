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
    static class SbmlTags
    {
        // namespaces
        public static string PMMLAB_NS = "pmmlab";
        public static string PMF_NS = "pmf";
        public static string DC_NS = "dc";
        public static string DCTERMS_NS = "dcterms";

        // pmmlab tags
        public static string COND_ID = "condID";
        public static string DATA_SOURCE = "dataSource";
        public static string GLOBAL_MODEL_ID = "globalModelID";
        public static string FORMULA_NAME = "formulaName";
        public static string SUBJECT = "subject";
        public static string PMMLAB_ID = "pmmlabId";
        public static string P = "P";
        public static string ERROR = "error";
        public static string T = "t";
        public static string CORRELATION = "correlation";
        public static string DESCRIPTION = "description";
        public static string IS_START = "isStart";
        public static string DETAIL = "detail";
        public static string ENVIRONMENT = "environment";
        public static string TRANSFORMATION = "transformation";
        public static string PRIMARY_MODEL = "primaryModel";

        // pmf tags
        public static string METADATA = "metadata";

        // dc tags
        public static string CREATOR = "creator";
        public static string TYPE = "type";
        public static string RIGHTS = "rights";
        public static string SOURCE = "source";

        // dcterms tags
        public static string CREATED = "created";
        public static string MODIFIED = "modified";

        // correlation attributes
        public static string ORIGNAME_ATTR = "origname";
        public static string VALUE_ATTR = "value";

        // reference attributes using RIS specification
        public static string RIS_AUTHOR = "AU";
        public static string RIS_YEAR = "PY";
        public static string RIS_TITLE = "TI";
        public static string RIS_ABSTRACT = "AB";
        public static string RIS_JOURNAL = "T2";
        public static string RIS_VOLUME = "VL";
        public static string RIS_ISSUE = "IS";
        public static string RIS_PAGE = "SP";
        public static string RIS_APPROVAL = "LB";
        public static string RIS_WEBSITE = "UR";
        public static string RIS_TYPE = "M3";
        public static string RIS_COMMENT = "N1";
    }

    public class CondIdNode
    {
        public XMLNode node { get; }

        public CondIdNode(int id)
        {
            node = new XMLNode(new XMLTriple(SbmlTags.COND_ID, null, SbmlTags.PMMLAB_NS));
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
    }

    public class Correlation
    {
        public string name { get; }
        public double value { get; }  // worths NaN when not set

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

        public override string ToString()
        {
            return "Correlation [name=" + name + ", value=" + value + "]";
        }

        // maybe equals should be implemented here
    }

    class DataSourceNode
    {
        public XMLNode node { get; }

        public DataSourceNode(XMLNode node)
        {
            this.node = node;
        }

        public DataSourceNode(string dataName)
        {
            XMLTriple triple = new XMLTriple(SbmlTags.DATA_SOURCE, null, SbmlTags.PMMLAB_NS);

            XMLAttributes attrs = new XMLAttributes();
            attrs.add("id", "source1");
            attrs.add("href", dataName);

            node = new XMLNode(triple, attrs);
        }

        public string getFile()
        {
            return node.getAttrValue("href");
        }
    }

    class GlobalModelIdNode
    {
        public XMLNode node { get; }

        public GlobalModelIdNode(int id)
        {
            node = new XMLNode(new XMLTriple(SbmlTags.GLOBAL_MODEL_ID, null, SbmlTags.PMMLAB_NS));
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
    }

    /// <summary>Limit values of a parameter.</summary>
    class Limits
    {
        public string var { get; }
        public double min { get; }
        public double max { get; }

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
    }

    class LimitsConstraint
    {
        const int LEVEL = 3;
        const int VERSION = 1;
        public Constraint constraint { get; }

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
    public class Metadata
    {
        public string givenName { get; set; }
        public string familyName { get; set; }
        public string contact { get; set; }
        public string createdDate { get; set; }
        public string modifiedDate { get; set; }
        public ModelType? type { get; set; }
        public string rights { get; set; }
        public string referenceLink { get; set; }
    }

    class MetadataAnnotation
    {
        public Metadata metadata { get; }
        public XMLNode annotation { get; }

        public MetadataAnnotation(Metadata metadata)
        {
            XMLTriple pmfTriple = new XMLTriple(SbmlTags.METADATA, "", SbmlTags.PMF_NS);
            annotation = new XMLNode(pmfTriple);

            // Builds creator node
            if (!string.IsNullOrEmpty(metadata.givenName) ||
                !string.IsNullOrEmpty(metadata.familyName) ||
                !string.IsNullOrEmpty(metadata.contact))
            {
                string givenName = string.IsNullOrEmpty(metadata.givenName) ? "" : metadata.givenName;
                string familyName = string.IsNullOrEmpty(metadata.familyName) ? "" : metadata.familyName;
                string contact = string.IsNullOrEmpty(metadata.contact) ? "" : metadata.contact;

                string creator = givenName = "." + familyName + "." + contact;
                XMLNode node = new XMLNode(new XMLTriple(SbmlTags.CREATOR, null, SbmlTags.DC_NS));
                node.addChild(new XMLNode(creator));
                annotation.addChild(node);
            }

            // Builds created date node
            if (!string.IsNullOrEmpty(metadata.createdDate))
            {
                XMLTriple triple = new XMLTriple(SbmlTags.CREATED, "", SbmlTags.DC_NS);
                XMLNode node = new XMLNode(triple);
                node.addChild(new XMLNode(metadata.createdDate));
                annotation.addChild(node);
            }

            // Builds modified date node
            if (!string.IsNullOrEmpty(metadata.modifiedDate))
            {
                XMLTriple triple = new XMLTriple(SbmlTags.MODIFIED, "", SbmlTags.DCTERMS_NS);
                XMLNode node = new XMLNode(triple);
                node.addChild(new XMLNode(metadata.modifiedDate));
                annotation.addChild(node);
            }

            // Builds type node
            if (metadata.type.HasValue)
            {
                XMLTriple triple = new XMLTriple(SbmlTags.TYPE, "", SbmlTags.DC_NS);
                XMLNode node = new XMLNode(triple);
                node.addChild(new XMLNode(metadata.type.ToString()));
                annotation.addChild(node);
            }

            // Builds rights node
            if (!string.IsNullOrEmpty(metadata.rights))
            {
                XMLTriple triple = new XMLTriple(SbmlTags.RIGHTS, "", SbmlTags.DC_NS);
                XMLNode node = new XMLNode(triple);
                node.addChild(new XMLNode(metadata.rights));
                annotation.addChild(node);
            }

            // Builds reference node
            if (!string.IsNullOrEmpty(metadata.referenceLink))
            {
                XMLTriple triple = new XMLTriple(SbmlTags.SOURCE, "", SbmlTags.DC_NS);
                XMLNode node = new XMLNode(triple);
                node.addChild(new XMLNode(metadata.referenceLink));
                annotation.addChild(node);
            }

            // Copies metadata
            this.metadata = metadata;
        }

        public MetadataAnnotation(XMLNode node)
        {
            annotation = node;

            if (node.hasChild(SbmlTags.CREATOR))
            {
                XMLNode creatorNode = node.getChild(SbmlTags.CREATOR);
                string chars = creatorNode.getChild(0).getCharacters();
                string[] tempStrings = chars.Split('.');

                if (!string.IsNullOrEmpty(tempStrings[0]))
                {
                    metadata.givenName = tempStrings[0];
                }
                if (!string.IsNullOrEmpty(tempStrings[1]))
                {
                    metadata.familyName = tempStrings[1];
                }
                if (!string.IsNullOrEmpty(tempStrings[2]))
                {
                    metadata.contact = tempStrings[2];
                }
            }

            if (node.hasChild(SbmlTags.CREATED))
            {
                XMLNode createdNode = node.getChild(SbmlTags.CREATED);
                metadata.createdDate = createdNode.getChild(0).getCharacters();
            }

            if (node.hasChild(SbmlTags.MODIFIED))
            {
                XMLNode modifiedNode = node.getChild(SbmlTags.MODIFIED);
                metadata.modifiedDate = modifiedNode.getChild(0).getCharacters();
            }

            if (node.hasChild(SbmlTags.TYPE))
            {
                XMLNode typeNode = node.getChild(SbmlTags.TYPE);

                Type enumType = Type.GetType("pmfml-cs.ModelType");
                ModelType type = (ModelType)Enum.Parse(enumType, typeNode.getChild(0).getCharacters());
                metadata.setType(type);
            }

            // rights node
            if (node.hasChild(SbmlTags.RIGHTS))
            {
                XMLNode rightsNode = node.getChild(SbmlTags.RIGHTS);
                metadata.rights = rightsNode.getChild(0).getCharacters();
            }

            // reference node
            if (node.hasChild(SbmlTags.SOURCE))
            {
                XMLNode referenceNode = node.getChild(SbmlTags.SOURCE);
                metadata.referenceLink = referenceNode.getChild(0).getCharacters();
            }
        }
    }

    /// <summary>
    /// Primary model annotation. Holds model id, model title, uncertainties,
    /// references and condId.
    /// </summary>
    public class Model1Annotation
    {
        public Uncertainties uncertainties { get; }
        public List<Reference> references { get; }
        public int condId { get; }
        public XMLNode annotation { get; }

        /// <summary>
        /// Gets fields from existing primary model annotation
        /// </summary>
        public Model1Annotation(XMLNode annotation)
        {
            this.annotation = annotation;

            // Gets condId
            condId = new CondIdNode(annotation.getChild(SbmlTags.COND_ID)).getCondId();

            // Gets model quality annotation
            if (annotation.hasChild(UncertaintyNode.TAG))
            {
                XMLNode modelQualityNode = annotation.getChild(UncertaintyNode.TAG);
                uncertainties = new UncertaintyNode(modelQualityNode).getMeasures();
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

        public Model1Annotation(Uncertainties uncertainties, List<Reference> references, int condId)
        {
            // Builds metadata node
            XMLTriple triple = new XMLTriple(SbmlTags.METADATA, "", SbmlTags.PMF_NS);
            annotation = new XMLNode(triple);

            // Build uncertainties node
            annotation.addChild(new UncertaintyNode(uncertainties).getNode());

            // Builds references nodes
            foreach (Reference reference in references)
            {
                annotation.addChild(new ReferenceSBMLNode(reference).node);
            }

            // Builds condID node
            annotation.addChild(new CondIdNode(condId).node);

            // Saves fields
            this.uncertainties = uncertainties;
            this.references = references;
            this.condId = condId;
        }
    }

    /// <summary>
    /// Secondary model annotation. Holds global model ID, references and
    /// uncertainty measures.
    /// </summary>
    public class Model2Annotation
    {
        public List<Reference> references { get; }
        public int globalModelID { get; }
        public Uncertainties uncertainties { get; }
        public XMLNode annotation { get; }

        /// <summary>
        /// Gets global model id, uncertainties and literature items of the
        /// model.
        /// </summary>
        public Model2Annotation(XMLNode annotation)
        {
            this.annotation = annotation;

            // Gets global model ID
            globalModelID = new GlobalModelIdNode(
                annotation.getChild(SbmlTags.GLOBAL_MODEL_ID)).getGlobalModelId();

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
            annotation = new XMLNode(new XMLTriple(SbmlTags.METADATA, "", SbmlTags.PMF_NS));

            // Builds GlobalModelIdNode
            annotation.addChild(new GlobalModelIdNode(globalModelID).node);

            // Builds UncertaintyNode
            annotation.addChild(new UncertaintyNode(uncertainties).getNode());

            // Builds reference nodes
            foreach (Reference reference in references)
            {
                annotation.addChild(new ReferenceSBMLNode(reference).node);
            }

            // Saves fields
            this.globalModelID = globalModelID;
            this.references = references;
            this.uncertainties = uncertainties;
        }
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
            rule.setAnnotation(annot.annotation);
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
        public XMLNode annotation { get; }

        public ModelRuleAnnotation(XMLNode annotation)
        {
            // Gets formula node
            XMLNode nameNode = annotation.getChild(SbmlTags.FORMULA_NAME);
            formulaName = nameNode.getChild(0).getCharacters();

            // Gets formula subject
            if (annotation.hasChild(SbmlTags.SUBJECT))
            {
                // TODO: set annotation from string in node ...
            }
            else
            {
                modelClass = ModelClass.UNKNOW;
            }

            // Gets PmmLab ID
            if (annotation.hasChild(SbmlTags.PMMLAB_ID))
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
            annotation = new XMLNode(new XMLTriple(SbmlTags.METADATA, "", SbmlTags.PMF_NS));

            // Creates annotation for formula name
            XMLTriple nameTriple = new XMLTriple(SbmlTags.FORMULA_NAME, "", SbmlTags.PMMLAB_NS);
            XMLNode nameNode = new XMLNode(nameTriple);
            nameNode.addChild(new XMLNode(formulaName));
            annotation.addChild(nameNode);

            // Creates annotation for modelClass
            XMLTriple modelClassTriple = new XMLTriple(SbmlTags.SUBJECT, "", SbmlTags.PMMLAB_NS);
            XMLNode modelClassNode = new XMLNode(modelClassTriple);
            modelClassNode.addChild(new XMLNode(Extensions.modelClassName(modelClass)));
            annotation.addChild(modelClassNode);

            // Creates annotation for pmmlabId
            XMLTriple idTriple = new XMLTriple(SbmlTags.PMMLAB_ID, "", SbmlTags.PMMLAB_NS);
            XMLNode idNode = new XMLNode(idTriple);
            idNode.addChild(new XMLNode(pmmlabId.ToString()));

            // Builds reference nodes
            foreach (Reference reference in references)
            {
                annotation.addChild(new ReferenceSBMLNode(reference).node);
            }

            // Saves formulaName, subject and model literature
            this.formulaName = formulaName;
            this.modelClass = modelClass;
            this.pmmLabId = pmmlabId;
            this.references = references;
        }
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
    public class PmfCoefficient
    {
        private const int LEVEL = 3;
        private const int VERSION = 1;

        public Parameter parameter { get; }
        public double p { get; set; }
        public double error { get; set; }
        public double t { get; set; }
        public List<Correlation> correlations { get; set; }
        public string description { get; set; }
        public bool isStart { get; set; }

        public PmfCoefficient(Parameter parameter)
        {

            this.parameter = parameter;

            // Parses annotation
            XMLNode annotation = parameter.getAnnotation();

            // Gets P
            if (annotation.hasChild(SbmlTags.P))
            {
                XMLNode pNode = annotation.getChild(SbmlTags.P);
                p = double.Parse(pNode.getChild(0).getCharacters());
            }

            // Gets error
            if (annotation.hasChild(SbmlTags.ERROR))
            {
                XMLNode errorNode = annotation.getChild(SbmlTags.ERROR);
                error = double.Parse(errorNode.getChild(0).getCharacters());
            }

            // Gets t
            if (annotation.hasChild(SbmlTags.T))
            {
                XMLNode tNode = annotation.getChild(SbmlTags.T);
                t = double.Parse(tNode.getChild(0).getCharacters());
            }

            // Gets correlations
            correlations = new List<Correlation>();
            for (int i = 0; i < annotation.getNumChildren(); i++)
            {
                XMLNode currentNode = annotation.getChild(i);
                if (currentNode.getName().Equals(SbmlTags.CORRELATION))
                {
                    XMLAttributes attrs = currentNode.getAttributes();
                    string corrName = attrs.getValue(SbmlTags.ORIGNAME_ATTR);

                    if (attrs.hasAttribute(SbmlTags.VALUE_ATTR))
                    {
                        string valueAsString = attrs.getValue(SbmlTags.VALUE_ATTR);
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
            if (annotation.hasChild(SbmlTags.DESCRIPTION))
            {
                XMLNode descNode = annotation.getChild(SbmlTags.DESCRIPTION);
                description = descNode.getChild(0).getCharacters();
            }

            // Gets isStart
            if (annotation.hasChild(SbmlTags.IS_START))
            {
                XMLNode isStartNode = annotation.getChild(SbmlTags.IS_START);
                isStart = bool.Parse(isStartNode.getChild(0).getCharacters());
            }
            else
            {
                isStart = false;
            }
        }

        public PmfCoefficient(string id, double value, string unit, double p,
            double error, double t, List<Correlation> correlations, string desc,
            bool isStart)
        {
            parameter = new Parameter(LEVEL, VERSION);
            parameter.setId(id);
            parameter.setValue(value);
            parameter.setUnits(unit);
            parameter.setConstant(true);

            XMLNode annotation = new XMLNode(new XMLTriple(SbmlTags.METADATA, "", SbmlTags.PMF_NS));

            // Creates P annotation
            if (!double.IsNaN(p))
            {
                XMLNode pNode = new XMLNode(new XMLTriple(SbmlTags.P, "", SbmlTags.PMMLAB_NS));
                pNode.addChild(new XMLNode(p.ToString()));
                annotation.addChild(pNode);
            }

            // Creates error annotation
            if (!double.IsNaN(error))
            {
                XMLNode errorNode = new XMLNode(new XMLTriple(SbmlTags.ERROR, "", SbmlTags.PMMLAB_NS));
                errorNode.addChild(new XMLNode(errorNode.ToString()));
                annotation.addChild(errorNode);
            }

            // Creates t annotation
            if (!double.IsNaN(t))
            {
                XMLNode tNode = new XMLNode(new XMLTriple(SbmlTags.T, "", SbmlTags.PMMLAB_NS));
                tNode.addChild(new XMLNode(tNode.ToString()));
                annotation.addChild(tNode);
            }

            // Creates Correlation annotations
            if (correlations != null)
            {
                foreach (Correlation correlation in correlations)
                {
                    XMLAttributes attrs = new XMLAttributes();
                    attrs.add(SbmlTags.ORIGNAME_ATTR, correlation.name);
                    if (!double.IsNaN(correlation.value))
                    {
                        attrs.add(SbmlTags.VALUE_ATTR, correlation.value.ToString());
                    }

                    XMLTriple triple = new XMLTriple(SbmlTags.CORRELATION, "", SbmlTags.PMMLAB_NS);
                    annotation.addChild(new XMLNode(triple, attrs));
                }
            }

            // Creates annotation for description
            if (!string.IsNullOrEmpty(desc))
            {
                XMLTriple descTriple = new XMLTriple(SbmlTags.DESCRIPTION, "", SbmlTags.PMMLAB_NS);
                XMLNode descNode = new XMLNode(descTriple);
                descNode.addChild(new XMLNode(desc));
                annotation.addChild(descNode);
            }

            // Creates annotation for isStart
            XMLTriple isStartTriple = new XMLTriple(SbmlTags.IS_START, "", SbmlTags.PMMLAB_NS);
            XMLNode isStartNode = new XMLNode(isStartTriple);
            isStartNode.addChild(new XMLNode(isStart.ToString()));
            annotation.addChild(isStartNode);

            parameter.setAnnotation(annotation);

            this.p = p;
            this.error = error;
            this.t = t;
            this.correlations = correlations;
            this.description = desc;
            this.isStart = isStart;
        }

        public PmfCoefficient(string id, double value, string unit) :
            this(id, value, unit, double.NaN, double.NaN, double.NaN, null, "", false)
        {
        }

        // --- id ---
        public string getId() { return parameter.getId(); }
        public void setId(string id) { parameter.setId(id); }

        // --- value ---
        public double getValue() { return parameter.getValue(); }
        public void setValue(double value) { parameter.setValue(value); }

        // --- unit ---
        public string getUnit() { return parameter.getUnits(); }
        public void setUnit(string unit) { parameter.setUnits(unit); }
    }

    public class PmfCompartment
    {
        private const int LEVEL = 3;
        private const int VERSION = 1;

        private const string ATTRIBUTE_NAME = "name";
        private const string ATTRIBUTE_VALUE = "value";

        public Compartment compartment { get; }
        public string pmfCode { get; set; }
        public string detail { get; set; }
        public List<ModelVariable> modelVariables { get; set; }

        public PmfCompartment(Compartment compartment)
        {
            this.compartment = compartment;

            if (compartment.isSetAnnotation())
            {
                XMLNode annot = compartment.getAnnotation();

                // Gets PMF code
                if (annot.hasChild(SbmlTags.SOURCE))
                {
                    XMLNode codeNode = annot.getChild(SbmlTags.SOURCE);
                    pmfCode = codeNode.getChild(0).getCharacters();
                }

                // Gets details
                if (annot.hasChild(SbmlTags.DETAIL))
                {
                    XMLNode detailsNode = annot.getChild(SbmlTags.DETAIL);
                    detail = detailsNode.getChild(0).getCharacters();
                }

                // Gets model variables
                modelVariables = new List<ModelVariable>();
                for (int i = 0; i < annot.getNumChildren(); i++)
                {
                    XMLNode currentNode = annot.getChild(i);
                    if (currentNode.getName().Equals(SbmlTags.ENVIRONMENT))
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

        public PmfCompartment(string id, string name, string pmfCode, string detail,
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
            XMLNode annot = new XMLNode(new XMLTriple(SbmlTags.METADATA, "", SbmlTags.PMMLAB_NS));

            // Creates annotation for the PMF code
            if (!string.IsNullOrEmpty(pmfCode))
            {
                XMLNode codeNode = new XMLNode(new XMLTriple(SbmlTags.SOURCE, "", SbmlTags.DC_NS));
                codeNode.addChild(new XMLNode(pmfCode));
                annot.addChild(codeNode);
            }

            // Creates annotation for the matrix details
            if (!string.IsNullOrEmpty(detail))
            {
                XMLNode detailsNode = new XMLNode(new XMLTriple(SbmlTags.DETAIL, "", SbmlTags.PMMLAB_NS));
                detailsNode.addChild(new XMLNode(detail));
                annot.addChild(detailsNode);
            }

            // Creates annotation for model variables (Temperature, pH, aW)
            if (modelVariables != null)
            {
                XMLTriple varTriple = new XMLTriple(SbmlTags.ENVIRONMENT, "", SbmlTags.PMMLAB_NS);
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

        public PmfCompartment(string id, string name)
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
    }

    public class PmfSpecies
    {
        public static bool BOUNDARY_CONDITION = true;
        public static bool CONSTANT = false;
        public static bool ONLY_SUBSTANCE_UNITS = false;

        const int LEVEL = 3;
        const int VERSION = 1;

        private Species species;
        public string combaseCode { get; set; }
        public string detail { get; set; }
        public string description { get; set; }

        public PmfSpecies(Species species)
        {
            this.species = species;

            if (species.isSetAnnotation())
            {
                XMLNode annot = species.getAnnotation();

                // Gets CAS number
                if (annot.hasChild(SbmlTags.SOURCE))
                {
                    XMLNode sourceNode = annot.getChild(SbmlTags.SOURCE);
                    string wholeReference = sourceNode.getChild(0).getCharacters();
                    combaseCode = wholeReference.Substring(wholeReference.LastIndexOf('/') + 1);
                }

                // Gets description
                if (annot.hasChild(SbmlTags.DETAIL))
                {
                    XMLNode detailNode = annot.getChild(SbmlTags.DETAIL);
                    detail = detailNode.getChild(0).getCharacters();
                }

                // Gets dep description
                if (annot.hasChild(SbmlTags.DESCRIPTION))
                {
                    XMLNode descNode = annot.getChild(SbmlTags.DESCRIPTION);
                    description = descNode.getChild(0).getCharacters();
                }
            }
        }

        public PmfSpecies(string compartment, string id, string name,
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
                XMLTriple triple = new XMLTriple(SbmlTags.METADATA, "", SbmlTags.PMMLAB_NS);
                XMLNode annot = new XMLNode(triple);

                // Builds reference tag
                if (!string.IsNullOrEmpty(combaseCode))
                {
                    XMLTriple refTriple = new XMLTriple(SbmlTags.SOURCE, "", SbmlTags.DC_NS);
                    XMLNode refNode = new XMLNode(refTriple);
                    refNode.addChild(new XMLNode("http://identifiers.org/ncim/" + combaseCode));
                    annot.addChild(refNode);

                    this.combaseCode = combaseCode;
                }

                // Builds detail tag
                if (!string.IsNullOrEmpty(detail))
                {
                    XMLTriple detailTriple = new XMLTriple(SbmlTags.DETAIL, "", SbmlTags.PMMLAB_NS);
                    XMLNode detailNode = new XMLNode(detailTriple);
                    annot.addChild(new XMLNode(detailNode));

                    this.detail = detail;
                }

                // Builds dep description tag
                if (!string.IsNullOrEmpty(description))
                {
                    XMLTriple descTriple = new XMLTriple(SbmlTags.DESCRIPTION, "", SbmlTags.PMMLAB_NS);
                    XMLNode descNode = new XMLNode(descTriple);
                    annot.addChild(descNode);

                    this.description = description;
                }
            }
        }

        // constructor with compartment, id, name and substanceUnits
        public PmfSpecies(string compartment, string id, string name, string substanceUnits)
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
                XMLTriple transformationTriple = new XMLTriple(SbmlTags.TRANSFORMATION, "", SbmlTags.PMMLAB_NS);
                XMLNode transformationNode = new XMLNode(transformationTriple);
                transformationNode.addChild(new XMLNode(transformationName));

                // Creates metadata node
                XMLTriple metadataTriple = new XMLTriple(SbmlTags.METADATA, "", SbmlTags.PMF_NS);
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
                XMLNode transformationNode = metadata.getChild(SbmlTags.TRANSFORMATION);
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
        public XMLNode node { get; }

        public PrimaryModelNode(string model)
        {
            node = new XMLNode(new XMLTriple(SbmlTags.PRIMARY_MODEL, "", SbmlTags.PMMLAB_NS));
            node.addChild(new XMLNode(model));
        }

        public PrimaryModelNode(XMLNode node)
        {
            this.node = node;
        }

        public string getPrimaryModel() { return node.getChild(0).getCharacters(); }
    }

    public class Reference
    {
        public string author { get; set; }
        public int year { get; set; }
        public string title { get; set; }
        public string abstractText { get; set; }
        public string journal { get; set; }
        public string volume { get; set; }
        public string issue { get; set; }
        public int? page { get; set; }
        public int? approvalMode { get; set; }
        public string website { get; set; }
        public ReferenceType? type { get; set; }
        public string comment { get; set; }
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

    public class ReferenceSBMLNode
    {
        public const string NS = "dc";
        public const string TAG = "reference";

        public XMLNode node { get; }

        public ReferenceSBMLNode(Reference reference)
        {
            // Reference container
            XMLNamespaces namespaces = new XMLNamespaces();
            namespaces.add("http://foo.bar.com", "ref");
            XMLTriple refTriple = new XMLTriple(TAG, "", NS);
            XMLNode refNode = new XMLNode(refTriple, null, namespaces);

            // author node
            if (!string.IsNullOrEmpty(reference.author))
            {
                XMLTriple triple = new XMLTriple(SbmlTags.RIS_AUTHOR, "", "ref");
                XMLNode node = new XMLNode(reference.author);
                refNode.addChild(node);
            }

            // title node
            if (!string.IsNullOrEmpty(reference.title))
            {
                XMLTriple triple = new XMLTriple(SbmlTags.RIS_TITLE, "", "ref");
                XMLNode node = new XMLNode(reference.title);
                refNode.addChild(node);
            }

            //  abstract node
            if (!string.IsNullOrEmpty(reference.abstractText))
            {
                XMLTriple triple = new XMLTriple(SbmlTags.RIS_ABSTRACT, "", "ref");
                XMLNode node = new XMLNode(reference.abstractText);
                refNode.addChild(node);
            }

            // journal node
            if (!string.IsNullOrEmpty(reference.journal))
            {
                XMLTriple triple = new XMLTriple(SbmlTags.RIS_JOURNAL, "", "ref");
                XMLNode node = new XMLNode(reference.journal);
                refNode.addChild(node);
            }

            // volume node
            if (!string.IsNullOrEmpty(reference.volume))
            {
                XMLTriple triple = new XMLTriple(SbmlTags.RIS_VOLUME, "", "ref");
                XMLNode node = new XMLNode(reference.volume);
                refNode.addChild(node);
            }

            // issue node
            if (!string.IsNullOrEmpty(reference.issue))
            {
                XMLTriple triple = new XMLTriple(SbmlTags.RIS_ISSUE, "", "ref");
                XMLNode node = new XMLNode(reference.issue);
                refNode.addChild(node);
            }

            // page
            if (reference.page.HasValue)
            {
                XMLTriple triple = new XMLTriple(SbmlTags.RIS_PAGE, "", "ref");
                XMLNode node = new XMLNode(reference.page.ToString());
                refNode.addChild(node);
            }

            // approval mode
            if (reference.approvalMode.HasValue)
            {
                XMLTriple triple = new XMLTriple(SbmlTags.RIS_ISSUE, "", "ref");
                XMLNode node = new XMLNode(reference.approvalMode.ToString());
                refNode.addChild(node);
            }

            // website
            if (!string.IsNullOrEmpty(reference.website))
            {
                XMLTriple triple = new XMLTriple(SbmlTags.RIS_WEBSITE, "", "ref");
                XMLNode node = new XMLNode(reference.website);
                refNode.addChild(node);
            }

            // type
            if (reference.type.HasValue)
            {
                XMLTriple triple = new XMLTriple(SbmlTags.RIS_TYPE, "", "ref");
                XMLNode node = new XMLNode(reference.type.ToString());
                refNode.addChild(node);
            }

            // comment
            if (!string.IsNullOrEmpty(reference.comment))
            {
                XMLTriple triple = new XMLTriple(SbmlTags.RIS_COMMENT, "", "ref");
                XMLNode node = new XMLNode(reference.comment);
                refNode.addChild(node);
            }
        }

        public ReferenceSBMLNode(XMLNode node)
        {
            this.node = node;
        }

        public Reference toReference()
        {
            Reference reference = new Reference();

            // author
            if (node.hasChild(SbmlTags.RIS_AUTHOR))
            {
                XMLNode authorNode = node.getChild(SbmlTags.RIS_AUTHOR);
                reference.author = authorNode.getChild(0).getCharacters();
            }

            // title
            if (node.hasChild(SbmlTags.RIS_TITLE))
            {
                XMLNode titleNode = node.getChild(SbmlTags.RIS_TITLE);
                reference.title = titleNode.getChild(0).getCharacters();
            }

            // abstract text
            if (node.hasChild(SbmlTags.RIS_ABSTRACT))
            {
                XMLNode abstractNode = node.getChild(SbmlTags.RIS_ABSTRACT);
                reference.abstractText = abstractNode.getChild(0).getCharacters();
            }

            // year
            if (node.hasChild(SbmlTags.RIS_YEAR))
            {
                XMLNode yearNode = node.getChild(SbmlTags.RIS_YEAR);
                reference.year = int.Parse(yearNode.getChild(0).getCharacters());
            }

            // journal
            if (node.hasChild(SbmlTags.RIS_JOURNAL))
            {
                XMLNode journalNode = node.getChild(SbmlTags.RIS_JOURNAL);
                reference.journal = journalNode.getChild(0).getCharacters();
            }

            // volume
            if (node.hasChild(SbmlTags.RIS_VOLUME))
            {
                XMLNode volumeNode = node.getChild(SbmlTags.RIS_VOLUME);
                reference.volume = volumeNode.getChild(0).getCharacters();
            }

            // issue
            if (node.hasChild(SbmlTags.RIS_ISSUE))
            {
                XMLNode issueNode = node.getChild(SbmlTags.RIS_ISSUE);
                reference.issue = issueNode.getChild(0).getCharacters();
            }

            // page
            if (node.hasChild(SbmlTags.RIS_PAGE))
            {
                XMLNode pageNode = node.getChild(SbmlTags.RIS_PAGE);
                reference.page = int.Parse(pageNode.getChild(0).getCharacters());
            }

            // approval mode
            if (node.hasChild(SbmlTags.RIS_APPROVAL))
            {
                XMLNode approvalNode = node.getChild(SbmlTags.RIS_APPROVAL);
                reference.approvalMode = int.Parse(approvalNode.getChild(0).getCharacters());
            }

            // website
            if (node.hasChild(SbmlTags.RIS_WEBSITE))
            {
                XMLNode websiteNode = node.getChild(SbmlTags.RIS_WEBSITE);
                reference.website = websiteNode.getChild(0).getCharacters();
            }

            // type
            if (node.hasChild(SbmlTags.RIS_TYPE))
            {
                XMLNode typeNode = node.getChild(SbmlTags.RIS_TYPE);
                string typeAsString = typeNode.getChild(0).getCharacters();
                if (typeAsString.Equals("paper"))
                {
                    reference.type = ReferenceType.Paper;
                }
                else if (typeAsString.Equals("sop"))
                {
                    reference.type = ReferenceType.SOP;
                }
                else if (typeAsString.Equals("la"))
                {
                    reference.type = ReferenceType.LA;
                }
                else if (typeAsString.Equals("handbuch"))
                {
                    reference.type = ReferenceType.Handbuch;
                }
                else if (typeAsString.Equals("laborbuch"))
                {
                    reference.type = ReferenceType.Laborbuch;
                }
                else if (typeAsString.Equals("buch"))
                {
                    reference.type = ReferenceType.Buch;
                }
                else if (typeAsString.Equals("berich"))
                {
                    reference.type = ReferenceType.Berich;
                }
            }

            // comment
            if (node.hasChild(SbmlTags.RIS_COMMENT))
            {
                XMLNode commentNode = node.getChild(SbmlTags.RIS_COMMENT);
                reference.comment = commentNode.getChild(0).getCharacters();
            }

            return reference;
        }
    }

    public class SBMLFactory
    {
        public static Metadata createMetadata()
        {
            return new Metadata();
        }

        public static Metadata createMetadata(string givenName,
            string familyName, string contact, string createdDate,
            string modifiedDate, ModelType type, string rights,
            string referenceLink)
        {
            Metadata metadata = new Metadata();
            metadata.givenName = givenName;
            metadata.familyName = familyName;
            metadata.contact = contact;
            metadata.createdDate = createdDate;
            metadata.modifiedDate = modifiedDate;
            metadata.type = type;
            metadata.rights = rights;
            metadata.referenceLink = referenceLink;

            return metadata;
        }

        public static PmfCoefficient createPMFCoefficient(Parameter parameter)
        {
            return new PmfCoefficient(parameter);
        }

        public static PmfCoefficient createPMFCoefficient(string id,
            double value, string unit, double p, double error, double t,
            List<Correlation> correlations, string desc, bool isStart)
        {
            return new PmfCoefficient(id, value, unit, p, error, t,
                correlations, desc, isStart);
        }

        public static PmfCompartment createPMFCompartment(Compartment compartment)
        {
            return new PmfCompartment(compartment);
        }

        public static PmfCompartment createPMFCompartment(string id, string name,
            string pmfCode, string detail, List<ModelVariable> modelVariables)
        {
            return new PmfCompartment(id, name, pmfCode, detail, modelVariables);
        }

        public static PmfCompartment createPMFCompartment(string id, string name)
        {
            return new PmfCompartment(id, name);
        }

        public static PmfSpecies createPMFSpecies(Species species)
        {
            return new PmfSpecies(species);
        }

        public static PmfSpecies createPMFSpecies(string compartment, string id,
            string name, string substanceUnits, string combaseCode,
            string detail, string description)
        {
            return new PmfSpecies(compartment, id, name, substanceUnits,
                combaseCode, detail, description);
        }

        public static PmfSpecies createPMFSpecies(string compartment,
            string id, string name, string substanceUnits)
        {
            return new PmfSpecies(compartment, id, name, substanceUnits);
        }

        /* A parametrized createReference (like in the Java library) does not
         * make sense in C# since nullables are not used and many of the
         * reference fields could be empty. Thus it is better to return an
         * empty reference and let the user populate it.
         */
        public static Reference createReference()
        {
            return new Reference();
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
