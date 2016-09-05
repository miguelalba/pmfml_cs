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
        public static string METADATA = "pmf:metadata";
        public static string SPECIES = "sbml:species";
        public static string UNIT_DEFINITION = "sbml:unitDefinition";
        public static string UNIT = "sbml:unit";
        public static string TIME_ONTOLOGY = "ontologyTerm";
        public static string TUPLE = "tuple";
        public static string TUPLE_DESCRIPTION = "tupleDescription";
        public static string REFERENCE = "dc:reference";
        public static string RESULT_COMPONENT = "resultComponent";
        public static string TRANSFORMATION = "pmf:transformation";

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
        private static string SOURCE_TAG = "dc:source";
        private static string DETAIL_TAG = "pmmlab:detail";
        private static string VARIABLE_TAG = "pmmlab:modelVariable";

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

                XmlElement metadata = doc.CreateElement(NuMLTags.METADATA);
                annotation.AppendChild(metadata);

                if (!string.IsNullOrEmpty(compartment.pmfCode))
                {
                    XmlElement pmfCodeNode = doc.CreateElement(SOURCE_TAG);
                    pmfCodeNode.InnerText = compartment.pmfCode;
                    metadata.AppendChild(pmfCodeNode);
                }

                if (!string.IsNullOrEmpty(compartment.detail))
                {
                    XmlElement detailNode = doc.CreateElement(DETAIL_TAG);
                    detailNode.InnerText = compartment.detail;
                    metadata.AppendChild(detailNode);
                }

                if (compartment.modelVariables != null)
                {
                    foreach (ModelVariable modelVariable in compartment.modelVariables)
                    {
                        XmlElement modelVariableNode = doc.CreateElement(VARIABLE_TAG);
                        modelVariableNode.SetAttribute("name", modelVariable.name);
                        if (!double.IsNaN(modelVariable.value))
                        {
                            modelVariableNode.SetAttribute("value", modelVariable.value.ToString());
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

            XmlElement metadataNode = (XmlElement)annotationElement.SelectSingleNode(NuMLTags.METADATA);

            XmlElement pmfCodeNode = (XmlElement)metadataNode.SelectSingleNode(SOURCE_TAG);
            string pmfCode = pmfCodeNode == null ? "" : pmfCodeNode.InnerText;

            XmlElement detailNode = (XmlElement)metadataNode.SelectSingleNode(DETAIL_TAG);
            string detail = detailNode == null ? "" : detailNode.InnerText;

            List<ModelVariable> modelVariables = null;
            XmlNodeList varNodes = metadataNode.SelectNodes(VARIABLE_TAG);
            if (varNodes.Count > 0)
            {
                modelVariables = new List<ModelVariable>(varNodes.Count);
                for (int i = 0; i < varNodes.Count; i++)
                {
                    XmlElement varElement = (XmlElement)varNodes.Item(i);
                    string varName = varElement.GetAttribute("name");
                    double varValue;
                    if (varElement.HasAttribute("value"))
                    {
                        varValue = double.Parse(varElement.GetAttribute("value"));
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

    class SpeciesNuMLNode
    {
        private static string SOURCE_TAG = "dc:source";
        private static string DETAIL_TAG = "pmmlab:detail";
        private static string DESCRIPTION_TAG = "pmmlab:description";

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

                XmlElement metadata = doc.CreateElement(NuMLTags.METADATA);
                annotation.AppendChild(metadata);

                if (!string.IsNullOrEmpty(species.combaseCode))
                {
                    XmlElement combaseCodeNode = doc.CreateElement(SOURCE_TAG);
                    combaseCodeNode.InnerText = species.combaseCode;
                    metadata.AppendChild(combaseCodeNode);
                }

                if (!string.IsNullOrEmpty(species.detail))
                {
                    XmlElement detailNode = doc.CreateElement(DETAIL_TAG);
                    detailNode.InnerText = species.detail;
                    metadata.AppendChild(detailNode);
                }

                if (!string.IsNullOrEmpty(species.description))
                {
                    XmlElement descriptionNode = doc.CreateElement(DESCRIPTION_TAG);
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
                XmlElement metadataNode = (XmlElement)annotationNode.SelectSingleNode(NuMLTags.METADATA);

                // combase code
                XmlElement combaseCodeNode = (XmlElement)metadataNode.SelectSingleNode(SOURCE_TAG);
                if (combaseCodeNode != null)
                {
                    species.combaseCode = combaseCodeNode.InnerText;
                }

                // detail 
                XmlElement detailNode = (XmlElement)metadataNode.SelectSingleNode(DETAIL_TAG);
                if (detailNode != null)
                {
                    species.detail = detailNode.InnerText;
                }

                // description
                XmlElement descriptionNode = (XmlElement)metadataNode.SelectSingleNode(DESCRIPTION_TAG);
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
        private static string CONDID = "pmmlab:condID";
        private static string COMBASEID = "pmmlab:combaseID";
        private static string CREATOR_GIVEN_NAME = "pmmlab:creatorGivenName";
        private static string CREATOR_FAMILY_NAME = "pmmlab:creatorFamilyName";
        private static string CREATOR_CONTACT = "pmmlab:creatorContact";
        private static string CREATED_DATE = "pmmlab:createdDate";
        private static string MODIFIED_DATE = "pmmlab:modifiedDate";
        private static string MODEL_TYPE = "pmmlab:modelType";
        private static string RIGHTS = "pmmlab:rights";

        private static string DIMENSION_DESCRIPTION = "dimensionDescription";
        private static string DIMENSION = "dimension";

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
            XmlElement metadataNode = (XmlElement)annotationNode.SelectSingleNode(NuMLTags.METADATA);

            // condId
            XmlElement condIdNode = (XmlElement)metadataNode.SelectSingleNode(CONDID);
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
            XmlElement createdDateNode = (XmlElement)metadataNode.SelectSingleNode(CREATED_DATE);
            if (createdDateNode != null)
            {
                createdDate = createdDateNode.InnerText;
            }

            // modified date
            XmlElement modifiedDateNode = (XmlElement)metadataNode.SelectSingleNode(MODIFIED_DATE);
            if (modifiedDateNode != null)
            {
                modifiedDate = modifiedDateNode.InnerText;
            }

            // TODO: Model type

            // rights
            XmlElement rightsNode = (XmlElement)metadataNode.SelectSingleNode(RIGHTS);
            if (rightsNode != null)
            {
                rights = rightsNode.InnerText;
            }

            // references
            XmlNodeList refNodes = metadataNode.GetElementsByTagName(NuMLTags.REFERENCE);
            references = new List<Reference>(refNodes.Count);
            for (int i = 0; i < refNodes.Count; i++)
            {
                XmlElement refElement = (XmlElement)refNodes.Item(i);
                references.Add(new ReferenceNuMLNode(refElement).toReference());
            }

            // tuple description
            XmlElement dimDescNode = (XmlElement)element.SelectSingleNode(DIMENSION_DESCRIPTION);
            XmlElement tupleDescNode = (XmlElement)dimDescNode.SelectSingleNode(NuMLTags.TUPLE_DESCRIPTION);
            tupleDescription = new TupleDescription(tupleDescNode);

            // dimensions
            XmlElement dimNode = (XmlElement)element.SelectSingleNode(DIMENSION);
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

            XmlElement metadata = doc.CreateElement(NuMLTags.METADATA);
            element.AppendChild(metadata);

            if (condId.HasValue)
            {
                XmlElement condIdNode = doc.CreateElement(CONDID);
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
                XmlElement createdDateNode = doc.CreateElement(CREATED_DATE);
                createdDateNode.InnerText = createdDate;
                metadata.AppendChild(createdDateNode);
            }

            if (!string.IsNullOrEmpty(modifiedDate))
            {
                XmlElement modifiedDateNode = doc.CreateElement(MODIFIED_DATE);
                modifiedDateNode.InnerText = modifiedDate;
                metadata.AppendChild(modifiedDateNode);
            }

            // TODO: model type

            if (!string.IsNullOrEmpty(rights))
            {
                XmlElement rightsNode = doc.CreateElement(RIGHTS);
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

            XmlElement dimDescNode = doc.CreateElement(DIMENSION_DESCRIPTION);
            dimDescNode.AppendChild(tupleDescription.toNode(doc));
            element.AppendChild(dimDescNode);

            XmlElement dimNode = doc.CreateElement(DIMENSION);
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
        private static LiteratureSpecificationI SPEC = new RIS();
        private static string DC_URI = "http://purl.org/dc/elements/1.1/";

        public XmlElement element { get; }

        public ReferenceNuMLNode(Reference reference, XmlDocument doc)
        {

            element = doc.CreateElement(DC_URI, NuMLTags.REFERENCE);

            if (reference.isSetAuthor())
            {
                XmlElement authorNode = doc.CreateElement(SPEC.getAuthor());
                authorNode.InnerText = reference.getAuthor();
                element.AppendChild(authorNode);
            }

            if (reference.isSetYear())
            {
                XmlElement yearNode = doc.CreateElement(SPEC.getYear());
                yearNode.InnerText = reference.getYear().ToString();
                element.AppendChild(yearNode);
            }

            if (reference.isSetTitle())
            {
                XmlElement titleNode = doc.CreateElement(SPEC.getTitle());
                titleNode.InnerText = reference.getTitle();
                element.AppendChild(titleNode);
            }

            if (reference.isSetAbstractText())
            {
                XmlElement abstractNode = doc.CreateElement(SPEC.getAbstract());
                abstractNode.InnerText = reference.getAbstractText();
                element.AppendChild(abstractNode);
            }

            if (reference.isSetJournal())
            {
                XmlElement journalNode = doc.CreateElement(SPEC.getJournal());
                journalNode.InnerText = reference.getJournal();
                element.AppendChild(journalNode);
            }

            if (reference.isSetVolume())
            {
                XmlElement volumeNode = doc.CreateElement(SPEC.getVolume());
                volumeNode.InnerText = reference.getVolume();
                element.AppendChild(volumeNode);
            }

            if (reference.isSetIssue())
            {
                XmlElement issueNode = doc.CreateElement(SPEC.getIssue());
                issueNode.InnerText = reference.getIssue();
                element.AppendChild(issueNode);
            }

            if (reference.isSetPage())
            {
                XmlElement pageNode = doc.CreateElement(SPEC.getPage());
                pageNode.InnerText = reference.getPage().ToString();
                element.AppendChild(pageNode);
            }

            if (reference.isSetApprovalMode())
            {
                XmlElement approvalNode = doc.CreateElement(SPEC.getApproval());
                approvalNode.InnerText = reference.getApprovalMode().ToString();
                element.AppendChild(approvalNode);
            }

            if (reference.isSetWebsite())
            {
                XmlElement websiteNode = doc.CreateElement(SPEC.getWebsite());
                websiteNode.InnerText = reference.getWebsite();
                element.AppendChild(websiteNode);
            }

            if (reference.isSetType())
            {
                XmlElement typeNode = doc.CreateElement(SPEC.getType());
                typeNode.InnerText = reference.getType().ToString();
                element.AppendChild(typeNode);
            }

            if (reference.isSetComment())
            {
                XmlElement commentNode = doc.CreateElement(SPEC.getComment());
                commentNode.InnerText = reference.getComment();
                element.AppendChild(commentNode);
            }
        }

        public ReferenceNuMLNode(XmlElement element)
        {
            this.element = element;
        }

        public Reference toReference()
        {
            Reference reference = new ReferenceImpl();

            XmlElement authorNode = (XmlElement)element.SelectSingleNode(SPEC.getAuthor());
            if (authorNode != null)
            {
                reference.setAuthor(authorNode.InnerText);
            }

            XmlElement titleNode = (XmlElement)element.SelectSingleNode(SPEC.getTitle());
            if (titleNode != null)
            {
                reference.setTitle(titleNode.InnerText);
            }

            XmlElement abstractNode = (XmlElement)element.SelectSingleNode(SPEC.getAbstract());
            if (abstractNode != null)
            {
                reference.setAbstractText(abstractNode.InnerText);
            }

            XmlElement yearNode = (XmlElement)element.SelectSingleNode(SPEC.getYear());
            if (yearNode != null)
            {
                reference.setYear(int.Parse(yearNode.InnerText));
            }

            XmlElement journalNode = (XmlElement)element.SelectSingleNode(SPEC.getJournal());
            if (journalNode != null)
            {
                reference.setJournal(journalNode.InnerText);
            }

            XmlElement volumeNode = (XmlElement)element.SelectSingleNode(SPEC.getVolume());
            if (volumeNode != null)
            {
                reference.setVolume(volumeNode.InnerText);
            }

            XmlElement issueNode = (XmlElement)element.SelectSingleNode(SPEC.getIssue());
            if (issueNode != null)
            {
                reference.setIssue(issueNode.InnerText);
            }

            XmlElement pageNode = (XmlElement)element.SelectSingleNode(SPEC.getPage());
            if (pageNode != null)
            {
                reference.setPage(int.Parse(pageNode.InnerText));
            }

            XmlElement approvalNode = (XmlElement)element.SelectSingleNode(SPEC.getApproval());
            if (approvalNode != null)
            {
                reference.setApprovalMode(int.Parse(approvalNode.InnerText));
            }

            XmlElement websiteNode = (XmlElement)element.SelectSingleNode(SPEC.getWebsite());
            if (websiteNode != null)
            {
                reference.setWebsite(websiteNode.InnerText);
            }

            // TODO: type

            // comment
            XmlElement commentNode = (XmlElement)element.SelectSingleNode(SPEC.getComment());
            if (commentNode != null)
            {
                reference.setComment(commentNode.InnerText);
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
                XmlElement transformationNode = doc.CreateElement(NuMLTags.TRANSFORMATION);
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

            XmlElement transformationNameNode = (XmlElement)element.SelectSingleNode(NuMLTags.TRANSFORMATION);
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
