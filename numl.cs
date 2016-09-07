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
using System.Collections.Generic;
using System.IO;
using System.Xml;

using libsbmlcs;
using pmfml_cs.sbml;

namespace pmfml_cs.numl
{
    public static class NuMLTags
    {
        // Element names
        public static string ANNOTATION = "annotation";
        public static string ATOMIC_DESCRIPTION = "atomicDescription";
        public static string ATOMIC_VALUE = "atomicValue";
        public static string CONCENTRATION_ONTOLOGY = "ontologyTerm";
        public static string COMPARTMENT = "sbml:compartment";
        public static string SPECIES = "sbml:species";
        public static string UNIT_DEFINITION = "sbml:unitDefinition";
        public static string UNIT = "sbml:unit";
        public static string TIME_ONTOLOGY = "ontologyTerm";
        public static string TUPLE = "tuple";
        public static string TUPLE_DESCRIPTION = "tupleDescription";
        public static string RESULT_COMPONENT = "resultComponent";
        public static string DIMENSION_DESCRIPTION = "dimensionDescription";
        public static string DIMENSION = "dimension";

        // Unit attributes
        public static string EXPONENT_ATTR = "exponent";
        public static string KIND_ATTR = "kind";
        public static string MULTIPLIER_ATTR = "multiplier";
        public static string SCALE_ATTR = "scale";

        // Ontology term (concentration & time) attributes
        public static string ID = "id";
        public static string TERM = "term";
        public static string SOURCE_TERM_ID = "sourceTermId";
        public static string URI = "ontologyURI";

        // Species attributes
        public static string BOUNDARY_CONDITION_ATTR = "boundaryCondition";
        public static string COMPARTMENT_ATTR = "compartment";
        public static string CONSTANT_ATTR = "constant";
        public static string HAS_ONLY_SUBSTANCE_ATTR = "hasOnlySubstanceUnits";
        public static string SUBSTANCE_UNITS_ATTR = "substanceUnits";

        // SBase attributes
        public static string ID_ATTR = "id";
        public static string NAME_ATTR = "name";

        // AtomicDescription attributes
        public static string ONTOLOGY_TERM_ATTR = "ontologyTerm";
        public static string VALUE_TYPE_ATTR = "valueType";
    }

    public class AtomicDescription
    {
        public string name { get; }
        public string ontologyTermId { get; }
        public static DataType valueType = DataType.DOUBLE;

        public AtomicDescription(string name, string ontologyTermId)
        {
            this.name = name;
            this.ontologyTermId = ontologyTermId;
        }

        public AtomicDescription(XmlElement node)
        {
            XmlAttributeCollection attributes = node.Attributes;
            name = node.Attributes[NuMLTags.NAME_ATTR].Value;
            ontologyTermId = node.Attributes[NuMLTags.ONTOLOGY_TERM_ATTR].Value;
        }

        // TODO: ToString
        // TODO: Equals

        public XmlElement ToNode(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement(NuMLTags.ATOMIC_DESCRIPTION);
            node.SetAttribute(NuMLTags.NAME_ATTR, name);
            node.SetAttribute(NuMLTags.ONTOLOGY_TERM_ATTR, ontologyTermId);
            // TODO: check once VALUE_TYPE is implemented
            node.SetAttribute(NuMLTags.VALUE_TYPE_ATTR, valueType.name());

            return node;
        }
    }

    public class AtomicValue
    {
        public double value { get; }

        public AtomicValue(double value)
        {
            this.value = value;
        }

        public AtomicValue(XmlElement node)
        {
            value = double.Parse(node.InnerText);
        }

        // TODO: ToString
        // TODO: Equals

        public XmlElement ToNode(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement(NuMLTags.ATOMIC_VALUE);
            node.InnerText = value.ToString();

            return node;
        }
    }

    public class ConcentrationOntology
    {
        public static string ID = "concentration";
        public static string TERM = "concentration";
        public static string SOURCE_TERM_ID = "SBO:0000196";
        public static string URI = "http://www.ebi.ac.uk/sbo/";

        public PMFUnitDefinition unitDefinition { get; }
        public PmfCompartment compartment { get; }
        public PmfSpecies species { get; }

        public ConcentrationOntology(PMFUnitDefinition unitDefinition,
            PmfCompartment compartment, PmfSpecies species)
        {
            this.unitDefinition = unitDefinition;
            this.compartment = compartment;
            this.species = species;
        }

        public ConcentrationOntology(XmlNode node)
        {
            XmlNode annotationNode = node.SelectSingleNode(NuMLTags.ANNOTATION);

            // retrieves unitDefinition
            XmlElement unitNode = (XmlElement)annotationNode.SelectSingleNode(NuMLTags.UNIT_DEFINITION);
            UnitDefinitionNuMLNode unitNuMLNode = new UnitDefinitionNuMLNode(unitNode);
            unitDefinition = unitNuMLNode.ToPMFUnitDefinition();

            // retrieves compartment
            XmlElement compartmentNode = (XmlElement)annotationNode.SelectSingleNode(NuMLTags.COMPARTMENT);
            CompartmentNuMLNode compartmentNuMLNode = new CompartmentNuMLNode(compartmentNode);
            compartment = compartmentNuMLNode.ToPMFCompartment();

            // retrieves species
            XmlElement speciesNode = (XmlElement)annotationNode.SelectSingleNode(NuMLTags.SPECIES);
            SpeciesNuMLNode speciesNuMLNode = new SpeciesNuMLNode(speciesNode);
            species = speciesNuMLNode.toPMFSpecies();
        }

        // TODO: toString
        // TODO: Equals

        public XmlElement ToNode(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement(NuMLTags.CONCENTRATION_ONTOLOGY);
            node.SetAttribute(NuMLTags.ID, ID);
            node.SetAttribute(NuMLTags.TERM, TERM);
            node.SetAttribute(NuMLTags.SOURCE_TERM_ID, SOURCE_TERM_ID);
            node.SetAttribute(NuMLTags.URI, URI);

            XmlElement annotation = doc.CreateElement(NuMLTags.ANNOTATION);
            node.AppendChild(annotation);

            // Creates UnitDefinitionNuMLNode and adds it to the annotation
            annotation.AppendChild(new UnitDefinitionNuMLNode(unitDefinition, doc).element);

            // Creates CompartmentNuMLNode and adds it to the annotation
            annotation.AppendChild(new CompartmentNuMLNode(compartment, doc).element);

            // Creates SpeciesNuMLNode and adds it to the annotation
            annotation.AppendChild(new SpeciesNuMLNode(species, doc).element);

            return node;
        }
    }

    class CompartmentNuMLNode
    {
        public XmlElement element { get; }

        public CompartmentNuMLNode(PmfCompartment compartment, XmlDocument doc)
        {
            element = doc.CreateElement(NuMLTags.COMPARTMENT);
            element.SetAttribute(NuMLTags.ID_ATTR, compartment.getId());
            element.SetAttribute(NuMLTags.NAME_ATTR, compartment.getName());

            if (!string.IsNullOrEmpty(compartment.pmfCode) ||
                !string.IsNullOrEmpty(compartment.detail) ||
                compartment.modelVariables != null)
            {
                XmlElement annotation = doc.CreateElement(NuMLTags.ANNOTATION);
                element.AppendChild(annotation);

                XmlElement metadata = doc.CreateElement(SbmlTags.PMF_NS, SbmlTags.METADATA, "");
                annotation.AppendChild(metadata);

                if (!string.IsNullOrEmpty(compartment.pmfCode))
                {
                    XmlElement pmfCodeNode = doc.CreateElement(SbmlTags.PMF_NS, SbmlTags.SOURCE, "");
                    pmfCodeNode.InnerText = compartment.pmfCode;
                    metadata.AppendChild(pmfCodeNode);
                }

                if (!string.IsNullOrEmpty(compartment.detail))
                {
                    XmlElement detailNode = doc.CreateElement(SbmlTags.PMMLAB_NS, SbmlTags.DETAIL, "");
                    detailNode.InnerText = compartment.detail;
                    metadata.AppendChild(detailNode);
                }

                if (compartment.modelVariables != null)
                {
                    foreach (ModelVariable modelVariable in compartment.modelVariables)
                    {
                        XmlElement modelVariableNode = doc.CreateElement(SbmlTags.ENVIRONMENT);
                        modelVariableNode.SetAttribute(SbmlTags.ENVIRONMENT_NAME, modelVariable.name);
                        if (!double.IsNaN(modelVariable.value))
                        {
                            modelVariableNode.SetAttribute(SbmlTags.ENVIRONMENT_VALUE, modelVariable.value.ToString());
                        }
                        metadata.AppendChild(modelVariableNode);
                    }
                }
            }
        }

        public CompartmentNuMLNode(XmlElement element)
        {
            this.element = element;
        }


        public PmfCompartment ToPMFCompartment()
        {
            string id = element.Attributes[NuMLTags.ID_ATTR].Value;
            string name = element.Attributes[NuMLTags.NAME_ATTR].Value;

            XmlElement annotationElement = (XmlElement)element.SelectSingleNode(NuMLTags.ANNOTATION);
            if (annotationElement == null)
            {
                return SBMLFactory.createPMFCompartment(id, name);
            }

            XmlElement metadataNode = (XmlElement)annotationElement.SelectSingleNode(SbmlTags.PMF_NS + ":" + SbmlTags.METADATA);

            XmlElement pmfCodeNode = (XmlElement)metadataNode.SelectSingleNode(SbmlTags.PMF_NS + ":" + SbmlTags.SOURCE);
            string pmfCode = pmfCodeNode == null ? "" : pmfCodeNode.InnerText;

            XmlElement detailNode = (XmlElement)metadataNode.SelectSingleNode(SbmlTags.PMMLAB_NS + ":" + SbmlTags.DETAIL);
            string detail = detailNode == null ? "" : detailNode.InnerText;

            List<ModelVariable> modelVariables = null;
            XmlNodeList varNodes = metadataNode.SelectNodes(SbmlTags.ENVIRONMENT);
            if (varNodes.Count > 0)
            {
                modelVariables = new List<ModelVariable>(varNodes.Count);
                for (int i = 0; i < varNodes.Count; i++)
                {
                    XmlElement varElement = (XmlElement)varNodes.Item(i);
                    string varName = varElement.GetAttribute(SbmlTags.ENVIRONMENT_NAME);
                    double varValue;
                    if (varElement.HasAttribute(SbmlTags.ENVIRONMENT_VALUE))
                    {
                        varValue = double.Parse(varElement.GetAttribute(SbmlTags.ENVIRONMENT_VALUE));
                    }
                    else
                    {
                        varValue = double.NaN;
                    }
                    modelVariables.Add(new ModelVariable(varName, varValue));
                }
            }

            return SBMLFactory.createPMFCompartment(id, name, pmfCode, detail, modelVariables);
        }
    }

    public class NuMLDocument
    {
        private static string NUML_NAMESPACE = "http://www.numl.org/numl/level1/version1";
        private static int VERSION = 1;
        private static int LEVEL = 1;

        static string ELEMENT_NAME = "numl";

        public ConcentrationOntology concOntology { get; }
        public TimeOntology timeOntology { get; }
        public ResultComponent resultComponent { get; }

        public NuMLDocument(ConcentrationOntology concOntology, TimeOntology timeOntology,
            ResultComponent resultComponent)
        {
            this.concOntology = concOntology;
            this.timeOntology = timeOntology;
            this.resultComponent = resultComponent;
        }

        public NuMLDocument(XmlElement element)
        {
            // Gets concentration and time ontologies
            concOntology = new ConcentrationOntology((XmlElement)element.ChildNodes[0]);
            timeOntology = new TimeOntology((XmlElement)element.ChildNodes[1]);

            // Gets the result component
            XmlElement rcElement = (XmlElement)element.SelectSingleNode(NuMLTags.RESULT_COMPONENT);
            resultComponent = new ResultComponent(rcElement);
        }

        public XmlElement ToNode(XmlDocument doc)
        {
            XmlElement element = doc.CreateElement(ELEMENT_NAME, NUML_NAMESPACE);
            element.SetAttribute("version", VERSION.ToString());
            element.SetAttribute("level", LEVEL.ToString());
            element.SetAttribute("xmlns:pmf",
                "http://sourceforge.net/projects/microbialmodelingexchange/files/PMF-ML");
            element.SetAttribute("xmlns:sbml", "http://www.sbml.org/sbml/level3/version1/core");
            element.SetAttribute("xmlns:dc", "http://purl.org/dc/elements/1.1/");
            element.SetAttribute("xmlns:dcterms", "http://purl.org/dc/terms/");
            element.SetAttribute("xmlns:pmmlab",
                "http://sourceforge.net/projects/microbialmodelingexchange/files/PMF-ML");

            element.AppendChild(concOntology.ToNode(doc));
            element.AppendChild(timeOntology.toNode(doc));
            element.AppendChild(resultComponent.ToNode(doc));

            return element;
        }
    }

    public class NuMLReader
    {
        public static NuMLDocument read(Stream inStream)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(inStream);

            return new NuMLDocument(doc.DocumentElement);
        }

        public static NuMLDocument read(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            return new NuMLDocument(doc.DocumentElement);
        }
    }

    public class NuMLWriter
    {
        public static void write(NuMLDocument doc, string filename)
        {
            XmlDocument xmlDoc = new XmlDocument();
            doc.ToNode(xmlDoc);

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  "
            };
            xmlDoc.Save(XmlWriter.Create(filename, settings));
        }
    }

    class SpeciesNuMLNode
    {
        public XmlElement element;

        public SpeciesNuMLNode(PmfSpecies species, XmlDocument doc)
        {
            element = doc.CreateElement(NuMLTags.SPECIES);
            element.SetAttribute(NuMLTags.BOUNDARY_CONDITION_ATTR, PmfSpecies.BOUNDARY_CONDITION.ToString());
            element.SetAttribute(NuMLTags.HAS_ONLY_SUBSTANCE_ATTR, PmfSpecies.ONLY_SUBSTANCE_UNITS.ToString());
            element.SetAttribute(NuMLTags.COMPARTMENT_ATTR, species.getCompartment());
            element.SetAttribute(NuMLTags.ID_ATTR, species.getId());
            element.SetAttribute(NuMLTags.NAME_ATTR, species.getName());
            element.SetAttribute(NuMLTags.SUBSTANCE_UNITS_ATTR, species.getUnits());

            if (!string.IsNullOrEmpty(species.combaseCode) ||
                !string.IsNullOrEmpty(species.detail) ||
                !string.IsNullOrEmpty(species.description))
            {
                XmlElement annotation = doc.CreateElement(NuMLTags.ANNOTATION);
                element.AppendChild(annotation);

                XmlElement metadata = doc.CreateElement(SbmlTags.PMF_NS, SbmlTags.METADATA, "");
                annotation.AppendChild(metadata);

                if (!string.IsNullOrEmpty(species.combaseCode))
                {
                    XmlElement combaseCodeNode = doc.CreateElement(SbmlTags.PMF_NS, SbmlTags.SOURCE, "");
                    combaseCodeNode.InnerText = species.combaseCode;
                    metadata.AppendChild(combaseCodeNode);
                }

                if (!string.IsNullOrEmpty(species.detail))
                {
                    XmlElement detailNode = doc.CreateElement(SbmlTags.PMMLAB_NS, SbmlTags.DETAIL, "");
                    detailNode.InnerText = species.detail;
                    metadata.AppendChild(detailNode);
                }

                if (!string.IsNullOrEmpty(species.description))
                {
                    XmlElement descriptionNode = doc.CreateElement(SbmlTags.PMMLAB_NS, SbmlTags.DESCRIPTION, "");
                    descriptionNode.InnerText = species.description;
                    metadata.AppendChild(descriptionNode);
                }
            }
        }

        public SpeciesNuMLNode(XmlElement element)
        {
            this.element = element;
        }

        public PmfSpecies toPMFSpecies()
        {
            string id = element.GetAttribute(NuMLTags.ID_ATTR);
            string name = element.GetAttribute(NuMLTags.NAME_ATTR);
            string compartment = element.GetAttribute(NuMLTags.COMPARTMENT_ATTR);
            string substanceUnits = element.GetAttribute(NuMLTags.SUBSTANCE_UNITS_ATTR);
            PmfSpecies species = SBMLFactory.createPMFSpecies(compartment, id, name, substanceUnits);

            XmlElement annotationNode = (XmlElement)element.SelectSingleNode(NuMLTags.ANNOTATION);
            if (annotationNode != null)
            {
                // metadata
                XmlElement metadataNode = (XmlElement)annotationNode.SelectSingleNode(SbmlTags.PMF_NS + ":" + SbmlTags.METADATA);

                // combase code
                XmlElement combaseCodeNode = (XmlElement)metadataNode.SelectSingleNode(SbmlTags.PMF_NS + ":" + SbmlTags.SOURCE);
                if (combaseCodeNode != null)
                {
                    species.combaseCode = combaseCodeNode.InnerText;
                }

                // detail 
                XmlElement detailNode = (XmlElement)metadataNode.SelectSingleNode(SbmlTags.PMMLAB_NS + ":" + SbmlTags.DETAIL);
                if (detailNode != null)
                {
                    species.detail = detailNode.InnerText;
                }

                // description
                XmlElement descriptionNode = (XmlElement)metadataNode.SelectSingleNode(SbmlTags.PMMLAB_NS + ":" + SbmlTags.DESCRIPTION);
                if (descriptionNode != null)
                {
                    species.description = descriptionNode.InnerText;
                }
            }

            return species;
        }
    }

    public enum DataType { STRING, FLOAT, DOUBLE, INTEGER };

    // NuMLDocument
    // NuMLReader
    // NuMLWriter

    public class ResultComponent
    {
        private static string ID = "id";
        private static string COMBASEID = "pmmlab:combaseID";
        private static string CREATOR_GIVEN_NAME = "pmmlab:creatorGivenName";
        private static string CREATOR_FAMILY_NAME = "pmmlab:creatorFamilyName";
        private static string CREATOR_CONTACT = "pmmlab:creatorContact";

        public string id { get; set; }
        public int? condId { get; set; }
        public string combaseId { get; set; }
        public string creatorGivenName { get; set; }
        public string creatorFamilyName { get; set; }
        public string creatorContact { get; set; }
        public string createdDate { get; set; }
        public string modifiedDate { get; set; }
        public ModelType modelType { get; set; }
        public string rights { get; set; }
        public List<Reference> references { get; set; }
        public TupleDescription tupleDescription { get; set; }
        public List<Tuple> dimensions { get; set; }
        public string notes { get; set; }

        public ResultComponent() { }

        public ResultComponent(XmlElement element)
        {
            XmlElement annotationNode = (XmlElement)element.SelectSingleNode(NuMLTags.ANNOTATION);
            XmlElement metadataNode = (XmlElement)annotationNode.SelectSingleNode(SbmlTags.PMF_NS + ":" + SbmlTags.METADATA);

            // condId
            XmlElement condIdNode = (XmlElement)metadataNode.SelectSingleNode(SbmlTags.PMMLAB_NS + ":" + SbmlTags.SOURCE);
            if (condIdNode != null)
            {
                condId = int.Parse(condIdNode.InnerText);
            }

            // combaseId
            XmlElement combaseIdNode = (XmlElement)metadataNode.SelectSingleNode(COMBASEID);
            if (combaseIdNode != null)
            {
                combaseId = combaseIdNode.InnerText;
            }

            // creator given name
            XmlElement creatorGivenNameNode = (XmlElement)metadataNode.SelectSingleNode(CREATOR_GIVEN_NAME);
            if (creatorGivenNameNode != null)
            {
                creatorGivenName = creatorGivenNameNode.InnerText;
            }

            // creator family name
            XmlElement creatorFamilyNameNode = (XmlElement)metadataNode.SelectSingleNode(CREATOR_FAMILY_NAME);
            if (creatorFamilyNameNode != null)
            {
                creatorFamilyName = creatorFamilyNameNode.InnerText;
            }

            // creator contact
            XmlElement creatorContactNode = (XmlElement)metadataNode.SelectSingleNode(CREATOR_CONTACT);
            if (creatorContact != null)
            {
                creatorContact = creatorContactNode.InnerText;
            }

            // created date
            XmlElement createdDateNode = (XmlElement)metadataNode.SelectSingleNode(SbmlTags.PMMLAB_NS + ":" + SbmlTags.CREATED);
            if (createdDateNode != null)
            {
                createdDate = createdDateNode.InnerText;
            }

            // modified date
            XmlElement modifiedDateNode = (XmlElement)metadataNode.SelectSingleNode(SbmlTags.PMMLAB_NS + ":" + SbmlTags.MODIFIED);
            if (modifiedDateNode != null)
            {
                modifiedDate = modifiedDateNode.InnerText;
            }

            // TODO: Model type

            // rights
            XmlElement rightsNode = (XmlElement)metadataNode.SelectSingleNode(SbmlTags.PMMLAB_NS + ":" + SbmlTags.RIGHTS);
            if (rightsNode != null)
            {
                rights = rightsNode.InnerText;
            }

            // references
            XmlNodeList refNodes = metadataNode.GetElementsByTagName(SbmlTags.DC_NS + ":" + SbmlTags.REFERENCE);
            references = new List<Reference>(refNodes.Count);
            for (int i = 0; i < refNodes.Count; i++)
            {
                XmlElement refElement = (XmlElement)refNodes.Item(i);
                references.Add(new ReferenceNuMLNode(refElement).toReference());
            }

            // tuple description
            XmlElement dimDescNode = (XmlElement)element.SelectSingleNode(NuMLTags.DIMENSION_DESCRIPTION);
            XmlElement tupleDescNode = (XmlElement)dimDescNode.SelectSingleNode(NuMLTags.TUPLE_DESCRIPTION);
            tupleDescription = new TupleDescription(tupleDescNode);

            // dimensions
            XmlElement dimNode = (XmlElement)element.SelectSingleNode(NuMLTags.DIMENSION);
            XmlNodeList tupleNodes = dimNode.GetElementsByTagName(NuMLTags.TUPLE);
            dimensions = new List<Tuple>(tupleNodes.Count);
            for (int i = 0; i < tupleNodes.Count; i++)
            {
                dimensions.Add(new Tuple((XmlElement)tupleNodes.Item(i)));
            }
        }

        // TODO: ToString
        // TODO: Equals

        // TODO: ToNode
        public XmlElement ToNode(XmlDocument doc)
        {
            XmlElement element = doc.CreateElement(NuMLTags.RESULT_COMPONENT);
            element.SetAttribute(ID, id);

            XmlElement annotation = doc.CreateElement(NuMLTags.ANNOTATION);
            element.AppendChild(annotation);

            XmlElement metadata = doc.CreateElement(SbmlTags.PMF_NS, SbmlTags.METADATA, "");
            element.AppendChild(metadata);

            if (condId.HasValue)
            {
                XmlElement condIdNode = doc.CreateElement(SbmlTags.PMMLAB_NS, SbmlTags.SOURCE, "");
                condIdNode.InnerText = condId.ToString();
                metadata.AppendChild(condIdNode);
            }

            if (!string.IsNullOrEmpty(combaseId))
            {
                XmlElement combaseIdNode = doc.CreateElement(COMBASEID);
                combaseIdNode.InnerText = combaseId;
                metadata.AppendChild(combaseIdNode);
            }

            if (!string.IsNullOrEmpty(creatorGivenName))
            {
                XmlElement creatorGivenNameNode = doc.CreateElement(CREATOR_GIVEN_NAME);
                creatorGivenNameNode.InnerText = creatorGivenName;
                metadata.AppendChild(creatorGivenNameNode);
            }

            if (!string.IsNullOrEmpty(creatorFamilyName))
            {
                XmlElement creatorFamilyNameNode = doc.CreateElement(CREATOR_FAMILY_NAME);
                creatorFamilyNameNode.InnerText = creatorFamilyName;
                metadata.AppendChild(creatorFamilyNameNode);
            }

            if (!string.IsNullOrEmpty(createdDate))
            {
                XmlElement createdDateNode = doc.CreateElement(SbmlTags.PMMLAB_NS, SbmlTags.CREATED, "");
                createdDateNode.InnerText = createdDate;
                metadata.AppendChild(createdDateNode);
            }

            if (!string.IsNullOrEmpty(modifiedDate))
            {
                XmlElement modifiedDateNode = doc.CreateElement(SbmlTags.PMMLAB_NS, SbmlTags.MODIFIED, "");
                modifiedDateNode.InnerText = modifiedDate;
                metadata.AppendChild(modifiedDateNode);
            }

            // TODO: model type

            if (!string.IsNullOrEmpty(rights))
            {
                XmlElement rightsNode = doc.CreateElement(SbmlTags.PMMLAB_NS, SbmlTags.RIGHTS, "");
                rightsNode.InnerText = rights;
                metadata.AppendChild(rightsNode);
            }

            if (!string.IsNullOrEmpty(notes))
            {
                XmlElement notesNode = doc.CreateElement("notes");
                notesNode.InnerText = notes;
                metadata.AppendChild(notesNode);
            }

            if (references.Count != 0)
            {
                foreach (Reference r in references)
                {
                    metadata.AppendChild(new ReferenceNuMLNode(r, doc).element);
                }
            }

            XmlElement dimDescNode = doc.CreateElement(NuMLTags.DIMENSION_DESCRIPTION);
            dimDescNode.AppendChild(tupleDescription.toNode(doc));
            element.AppendChild(dimDescNode);

            XmlElement dimNode = doc.CreateElement(NuMLTags.DIMENSION);
            foreach (Tuple tuple in dimensions)
            {
                dimNode.AppendChild(tuple.toNode(doc));
            }
            element.AppendChild(dimNode);

            return element;
        }
    }

    class ReferenceNuMLNode
    {
        private static string DC_URI = "http://purl.org/dc/elements/1.1/";

        public XmlElement element { get; }

        public ReferenceNuMLNode(Reference reference, XmlDocument doc)
        {

            element = doc.CreateElement(SbmlTags.DC_NS, SbmlTags.REFERENCE, DC_URI);

            if (!string.IsNullOrEmpty(reference.author))
            {
                XmlElement authorNode = doc.CreateElement(SbmlTags.RIS_AUTHOR);
                authorNode.InnerText = reference.author;
                element.AppendChild(authorNode);
            }

            if (reference.year.HasValue)
            {
                XmlElement yearNode = doc.CreateElement(SbmlTags.RIS_YEAR);
                yearNode.InnerText = reference.year.ToString();
                element.AppendChild(yearNode);
            }

            if (!string.IsNullOrEmpty(reference.title))
            {
                XmlElement titleNode = doc.CreateElement(SbmlTags.RIS_TITLE);
                titleNode.InnerText = reference.title;
                element.AppendChild(titleNode);
            }

            if (!string.IsNullOrEmpty(reference.abstractText))
            {
                XmlElement abstractNode = doc.CreateElement(SbmlTags.RIS_ABSTRACT);
                abstractNode.InnerText = reference.abstractText;
                element.AppendChild(abstractNode);
            }

            if (!string.IsNullOrEmpty(reference.journal))
            {
                XmlElement journalNode = doc.CreateElement(SbmlTags.RIS_JOURNAL);
                journalNode.InnerText = reference.journal;
                element.AppendChild(journalNode);
            }

            if (!string.IsNullOrEmpty(reference.volume))
            {
                XmlElement volumeNode = doc.CreateElement(SbmlTags.RIS_VOLUME);
                volumeNode.InnerText = reference.volume;
                element.AppendChild(volumeNode);
            }

            if (!string.IsNullOrEmpty(reference.issue))
            {
                XmlElement issueNode = doc.CreateElement(SbmlTags.RIS_ISSUE);
                issueNode.InnerText = reference.issue;
                element.AppendChild(issueNode);
            }

            if (reference.page.HasValue)
            {
                XmlElement pageNode = doc.CreateElement(SbmlTags.RIS_PAGE);
                pageNode.InnerText = reference.page.ToString();
                element.AppendChild(pageNode);
            }

            if (reference.approvalMode.HasValue)
            {
                XmlElement approvalNode = doc.CreateElement(SbmlTags.RIS_APPROVAL);
                approvalNode.InnerText = reference.approvalMode.ToString();
                element.AppendChild(approvalNode);
            }

            if (!string.IsNullOrEmpty(reference.website))
            {
                XmlElement websiteNode = doc.CreateElement(SbmlTags.RIS_WEBSITE);
                websiteNode.InnerText = reference.website;
                element.AppendChild(websiteNode);
            }

            if (reference.isSetType())
            {
                XmlElement typeNode = doc.CreateElement(SbmlTags.RIS_TYPE);
                typeNode.InnerText = reference.type.ToString();
                element.AppendChild(typeNode);
            }

            if (!string.IsNullOrEmpty(reference.comment))
            {
                XmlElement commentNode = doc.CreateElement(SbmlTags.RIS_COMMENT);
                commentNode.InnerText = reference.comment;
                element.AppendChild(commentNode);
            }
        }

        public ReferenceNuMLNode(XmlElement element)
        {
            this.element = element;
        }

        public Reference toReference()
        {
            Reference reference = new Reference();

            XmlElement authorNode = (XmlElement)element.SelectSingleNode(SbmlTags.RIS_AUTHOR);
            if (authorNode != null)
            {
                reference.author = authorNode.InnerText;
            }

            XmlElement titleNode = (XmlElement)element.SelectSingleNode(SbmlTags.RIS_TITLE);
            if (titleNode != null)
            {
                reference.title = titleNode.InnerText;
            }

            XmlElement abstractNode = (XmlElement)element.SelectSingleNode(SbmlTags.RIS_ABSTRACT);
            if (abstractNode != null)
            {
                reference.abstractText = abstractNode.InnerText;
            }

            XmlElement yearNode = (XmlElement)element.SelectSingleNode(SbmlTags.RIS_YEAR);
            if (yearNode != null)
            {
                reference.year = int.Parse(yearNode.InnerText);
            }

            XmlElement journalNode = (XmlElement)element.SelectSingleNode(SbmlTags.RIS_JOURNAL);
            if (journalNode != null)
            {
                reference.journal = journalNode.InnerText;
            }

            XmlElement volumeNode = (XmlElement)element.SelectSingleNode(SbmlTags.RIS_VOLUME);
            if (volumeNode != null)
            {
                reference.volume = volumeNode.InnerText;
            }

            XmlElement issueNode = (XmlElement)element.SelectSingleNode(SbmlTags.RIS_ISSUE);
            if (issueNode != null)
            {
                reference.issue = issueNode.InnerText;
            }

            XmlElement pageNode = (XmlElement)element.SelectSingleNode(SbmlTags.RIS_PAGE);
            if (pageNode != null)
            {
                reference.page = int.Parse(pageNode.InnerText);
            }

            XmlElement approvalNode = (XmlElement)element.SelectSingleNode(SbmlTags.RIS_APPROVAL);
            if (approvalNode != null)
            {
                reference.approvalMode = int.Parse(approvalNode.InnerText);
            }

            XmlElement websiteNode = (XmlElement)element.SelectSingleNode(SbmlTags.RIS_WEBSITE);
            if (websiteNode != null)
            {
                reference.website = websiteNode.InnerText;
            }

            // TODO: type

            // comment
            XmlElement commentNode = (XmlElement)element.SelectSingleNode(SbmlTags.RIS_COMMENT);
            if (commentNode != null)
            {
                reference.comment = commentNode.InnerText;
            }

            return reference;
        }
    }

    public class TimeOntology
    {
        // TimeOntology constants
        public static string ID = "time";
        public static string TERM = "time";
        public static string SOURCE_TERM_ID = "SBO:0000345";
        public static string URI = "http://www.ebi.ac.uk/sbo";

        PMFUnitDefinition unitDefinition;

        public TimeOntology(PMFUnitDefinition unitDefinition)
        {
            this.unitDefinition = unitDefinition;
        }

        public TimeOntology(XmlElement element)
        {
            XmlElement annotationNode = (XmlElement)element.SelectSingleNode(NuMLTags.ANNOTATION);
            XmlElement unitNode = (XmlElement)annotationNode.SelectSingleNode(NuMLTags.UNIT_DEFINITION);
            unitDefinition = new UnitDefinitionNuMLNode(unitNode).ToPMFUnitDefinition();
        }

        public PMFUnitDefinition getUnitDefinition() { return unitDefinition; }

        // TODO: ToString
        // TODO: Equals

        public XmlElement toNode(XmlDocument doc)
        {
            XmlElement element = doc.CreateElement(NuMLTags.TIME_ONTOLOGY);
            element.SetAttribute(NuMLTags.ID, ID);
            element.SetAttribute(NuMLTags.TERM, TERM);
            element.SetAttribute(NuMLTags.SOURCE_TERM_ID, SOURCE_TERM_ID);
            element.SetAttribute(NuMLTags.URI, URI);

            XmlElement annotation = doc.CreateElement(NuMLTags.ANNOTATION);
            element.AppendChild(annotation);

            annotation.AppendChild(new UnitDefinitionNuMLNode(unitDefinition, doc).element);

            return element;
        }
    }

    public class Tuple
    {
        public AtomicValue concValue { get; }
        public AtomicValue timeValue { get; }

        public Tuple(AtomicValue concValue, AtomicValue timeValue)
        {
            this.concValue = concValue;
            this.timeValue = timeValue;
        }

        public Tuple(XmlElement element)
        {
            concValue = new AtomicValue((XmlElement)element.ChildNodes[0]);
            timeValue = new AtomicValue((XmlElement)element.ChildNodes[1]);
        }

        // TODO: ToString
        // TODO: Equals

        public XmlElement toNode(XmlDocument doc)
        {
            XmlElement element = doc.CreateElement(NuMLTags.TUPLE);
            element.AppendChild(concValue.ToNode(doc));
            element.AppendChild(timeValue.ToNode(doc));

            return element;
        }
    }

    public class TupleDescription
    {
        public AtomicDescription concDesc { get; }
        public AtomicDescription timeDesc { get; }

        public TupleDescription(AtomicDescription concDesc, AtomicDescription timeDesc)
        {
            this.concDesc = concDesc;
            this.timeDesc = timeDesc;
        }

        public TupleDescription(XmlElement element)
        {
            XmlNodeList nodes = element.GetElementsByTagName(NuMLTags.ATOMIC_DESCRIPTION);
            concDesc = new AtomicDescription((XmlElement)nodes.Item(0));
            timeDesc = new AtomicDescription((XmlElement)nodes.Item(1));
        }

        // TODO: ToString
        // TODO: Equals

        public XmlElement toNode(XmlDocument doc)
        {
            XmlElement element = doc.CreateElement(NuMLTags.TUPLE_DESCRIPTION);
            element.AppendChild(concDesc.ToNode(doc));
            element.AppendChild(timeDesc.ToNode(doc));

            return element;
        }
    }

    public class UnitDefinitionNuMLNode
    {
        public XmlElement element { get; }

        public UnitDefinitionNuMLNode(PMFUnitDefinition unitDefinition, XmlDocument doc)
        {
            element = doc.CreateElement(NuMLTags.UNIT_DEFINITION);
            element.SetAttribute(NuMLTags.ID_ATTR, unitDefinition.getId());
            element.SetAttribute(NuMLTags.NAME_ATTR, unitDefinition.getName());

            // Adds transformation name
            if (unitDefinition.isSetTransformationName())
            {
                XmlElement transformationNode = doc.CreateElement(SbmlTags.PMMLAB_NS + ":" + SbmlTags.TRANSFORMATION);
                transformationNode.InnerText = unitDefinition.transformationName;
                element.AppendChild(transformationNode);
            }

            // Adds units
            foreach (Unit unit in unitDefinition.getUnits())
            {
                element.AppendChild(new UnitNuMLNode(unit, doc).element);
            }
        }

        public UnitDefinitionNuMLNode(XmlElement element)
        {
            this.element = element;
        }

        public PMFUnitDefinition ToPMFUnitDefinition()
        {
            string id = element.GetAttribute(NuMLTags.ID_ATTR);
            string name = element.GetAttribute(NuMLTags.NAME_ATTR);

            XmlElement transformationNameNode = (XmlElement)element.SelectSingleNode(SbmlTags.PMMLAB_NS + ":" + SbmlTags.TRANSFORMATION);
            string transformationName = transformationNameNode == null ? "" : transformationNameNode.InnerText;

            XmlNodeList unitNodes = element.GetElementsByTagName(NuMLTags.UNIT);
            List<Unit> units = new List<Unit>(unitNodes.Count);
            for (int i = 0; i < unitNodes.Count; i++)
            {
                XmlElement unitNode = (XmlElement)unitNodes.Item(i);
                units.Add(new UnitNuMLNode(unitNode).toUnit());
            }

            return new PMFUnitDefinition(id, name, transformationName, units);
        }
    }

    class UnitNuMLNode
    {
        public XmlElement element { get; }

        public UnitNuMLNode(Unit unit, XmlDocument doc)
        {
            element = doc.CreateElement(NuMLTags.UNIT);
            element.SetAttribute(NuMLTags.EXPONENT_ATTR, unit.getExponent().ToString());
            element.SetAttribute(NuMLTags.KIND_ATTR, unit.getKind().ToString());
            element.SetAttribute(NuMLTags.MULTIPLIER_ATTR, unit.getMultiplier().ToString());
            element.SetAttribute(NuMLTags.SCALE_ATTR, unit.getScale().ToString());
        }

        public UnitNuMLNode(XmlElement element)
        {
            this.element = element;
        }

        public Unit toUnit()
        {
            Unit unit = new Unit(3, 1);
            unit.setExponent(double.Parse(element.GetAttribute(NuMLTags.EXPONENT_ATTR)));
            unit.setKind(int.Parse(element.GetAttribute(NuMLTags.KIND_ATTR)));
            unit.setMultiplier(double.Parse(element.GetAttribute(NuMLTags.MULTIPLIER_ATTR)));
            unit.setScale(int.Parse(element.GetAttribute(NuMLTags.SCALE_ATTR)));

            return unit;
        }
    }
}
