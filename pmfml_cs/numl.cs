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
    }

    public class AtomicDescription
    {
        private static string NAME = "name";
        private static string ONTOLOGY_TERM = "ontologyTerm";
        private static string VALUE_TYPE = "valueType";

        private string name;
        private string ontologyTermId;
        public static DataType valueType = DataType.DOUBLE;

        public AtomicDescription(string name, string ontologyTermId)
        {
            this.name = name;
            this.ontologyTermId = ontologyTermId;
        }

        public AtomicDescription(XmlElement node)
        {
            XmlAttributeCollection attributes = node.Attributes;
            name = node.Attributes[NAME].Value;
            valueType = node.Attributes[ONTOLOGY_TERM].Value;
        }

        public string getName() { return name; }
        public string getOntologyTermId() { return ontologyTermId; }

        // TODO: ToString
        // TODO: Equals

        public XmlElement ToNode(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement(NuMLTags.ATOMIC_DESCRIPTION);
            node.SetAttribute(NAME, name);
            node.SetAttribute(ONTOLOGY_TERM, ontologyTermId);
            // TODO: check once VALUE_TYPE is implemented
            node.SetAttribute(VALUE_TYPE, valueType.name());

            return node;
        }
    }

    public class AtomicValue
    {
        private double value;

        public AtomicValue(double value)
        {
            this.value = value;
        }

        public AtomicValue(XmlElement node)
        {
            value = double.Parse(node.InnerText);
        }

        public double getValue() { return value; }

        // TODO: ToString
        // TODO: Equals

        public XmlElement ToNode(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement(NuMLTags.ATOMIC_VALUE);
            node.InnerText = value.ToString();

            return node;
        }
    }

    /* ConcentrationOntology
     * - ConcentrationOntology
     * - CompartmentNuMLNode
     * - SpeciesNuMLNode
     */
    public class ConcentrationOntology
    {
        private static string ANNOTATION = "annotation";

        public static string ID = "concentration";
        public static string TERM = "concentration";
        public static string SOURCE_TERM_ID = "SBO:0000196";
        public static string URI = "http://www.ebi.ac.uk/sbo/";

        private PMFUnitDefinition unitDefinition;
        private PMFCompartment compartment;
        private PMFSpecies species;

        public ConcentrationOntology(PMFUnitDefinition unitDefinition,
            PMFCompartment compartment, PMFSpecies species)
        {
            this.unitDefinition = unitDefinition;
            this.compartment = compartment;
            this.species = species;
        }

        public ConcentrationOntology(XmlNode node)
        {
            XmlNode annotationNode = node.SelectSingleNode(ANNOTATION);

            // retrieves unitDefinition
            XmlNode unitNode = annotationNode.SelectSingleNode(UnitDefinitionNuMLNode.TAG);
            UnitDefinitionNuMLNode unitNuMLNode = new UnitDefinitionNuMLNode(unitNode);
            unitDefinition = unitNuMLNode.ToPmfUnitDefinition();

            // retrieves compartment
            XmlNode compartmentNode = annotationNode.SelectSingleNode(CompartmentNuMLNode.TAG);
            CompartmentNuMLNode compartmentNuMLNode = new CompartmentNuMLNode(compartmentNode);
            compartment = compartmentNuMLNode.ToPmfCompartment();

            // retrieves species
            XmlNode speciesNode = annotationNode.SelectSingleNode(SpeciesNuMLNode.TAG);
            SpeciesNuMLNode speciesNuMLNode = new SpeciesNuMLNode(speciesNode);
            species = speciesNuMLNode.ToPmfSpecies();
        }

        public PMFUnitDefinition getUnitDefinition() { return unitDefinition; }
        public PMFCompartment getCompartment() { return compartment; }
        public PMFSpecies getSpecies() { return species; }

        // TODO: toString
        // TODO: Equals

        public XmlElement ToNode(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement(NuMLTags.CONCENTRATION_ONTOLOGY);
            node.SetAttribute(NuMLTags.ID, ID);
            node.SetAttribute(NuMLTags.TERM, TERM);
            node.SetAttribute(NuMLTags.SOURCE_TERM_ID, SOURCE_TERM_ID);
            node.SetAttribute(NuMLTags.URI, URI);

            XmlElement annotation = doc.CreateElement(ANNOTATION);
            node.AppendChild(annotation);

            // Creates UnitDefinitionNuMLNode and adds it to the annotation
            annotation.AppendChild(new UnitDefinitionNuMLNode(unitDefinition, doc).node);

            // Creates CompartmentNuMLNode and adds it to the annotation
            annotation.AppendChild(new CompartmentNuMLNode(compartment, doc).node);

            // Creates SpeciesNuMLNode and adds it to the annotation
            annotation.AppendChild(new SpeciesNuMLNode(species, doc).node);

            return node;
        }
    }

    class CompartmentNuMLNode
    {
        private static string ANNOTATION = "annotation";
        private static string METADATA = "pmf:metadata";

        private static string SOURCE_TAG = "dc:source";
        private static string DETAIL_TAG = "pmmlab:detail";
        private static string VARIABLE_TAG = "pmmlab:modelVariable";

        private static string ID_ATTR = "id";
        private static string NAME_ATTR = "name";

        XmlElement element;

        public CompartmentNuMLNode(PMFCompartment compartment, XmlDocument doc)
        {
            element = doc.CreateElement(NuMLTags.COMPARTMENT);
            element.SetAttribute(ID_ATTR, compartment.getId());
            element.SetAttribute(NAME_ATTR, compartment.getName());

            if (compartment.isSetPmfCode() || compartment.isSetDetail()
                || compartment.isSetModelVariables())
            {
                XmlElement annotation = doc.CreateElement(ANNOTATION);
                element.AppendChild(annotation);

                XmlElement metadata = doc.CreateElement(METADATA);
                annotation.AppendChild(metadata);

                if (compartment.isSetPmfCode())
                {
                    XmlElement pmfCodeNode = doc.CreateElement(SOURCE_TAG);
                    pmfCodeNode.InnerText = compartment.getPmfCode();
                    metadata.AppendChild(pmfCodeNode);
                }

                if (compartment.isSetDetail())
                {
                    XmlElement detailNode = doc.CreateElement(DETAIL_TAG);
                    detailNode.InnerText = compartment.getDetail();
                    metadata.AppendChild(detailNode);
                }

                if (compartment.isSetModelVariables())
                {
                    foreach (ModelVariable modelVariable in compartment.getModelVariables())
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
        public PMFCompartment toPMFCompartment()
        {
            string id = element.Attributes[ID_ATTR].Value;
            string name = element.Attributes[NAME_ATTR].Value;

            XmlElement annotationElement = (XmlElement)element.SelectSingleNode(ANNOTATION);
            if (annotationElement == null)
            {
                return SBMLFactory.createPMFCompartment(id, name);
            }

            XmlElement metadataNode = (XmlElement)annotationElement.SelectSingleNode(METADATA);

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
        private static string ANNOTATION = "annotation";
        private static string METADATA = "pmf:metadata";

        private static string BOUNDARY_CONDITION_ATTR = "boundaryCondition";
        private static string COMPARTMENT_ATTR = "compartment";
        private static string CONSTANT_ATTR = "constant";
        private static string HAS_ONLY_SUBSTANCE_UNITS_ATTR = "hasOnlySubstanceUnits";
        private static string ID_ATTR = "id";
        private static string NAME_ATTR = "name";
        private static string SUBSTANCE_UNITS_ATTR = "substanceUnits";

        private static string SOURCE_TAG = "dc:source";
        private static string DETAIL_TAG = "pmmlab:detail";
        private static string DESCRIPTION_TAG = "pmmlab:description";

        XmlElement element;

        public SpeciesNuMLNode(PMFSpecies species, XmlDocument doc)
        {
            element = doc.CreateElement(NuMLTags.SPECIES);
            element.SetAttribute(BOUNDARY_CONDITION_ATTR, PMFSpeciesImpl.BOUNDARY_CONDITION.ToString());
            element.SetAttribute(HAS_ONLY_SUBSTANCE_UNITS_ATTR, PMFSpeciesImpl.ONLY_SUBSTANCE_UNITS.ToString());
            element.SetAttribute(COMPARTMENT_ATTR, species.getCompartment());
            element.SetAttribute(ID_ATTR, species.getId());
            element.SetAttribute(NAME_ATTR, species.getName());
            element.SetAttribute(SUBSTANCE_UNITS_ATTR, species.getUnits());

            if (species.isSetCombaseCode() || species.isSetDetail() || species.isSetDescription())
            {
                XmlElement annotation = doc.CreateElement(ANNOTATION);
                element.AppendChild(annotation);

                XmlElement metadata = doc.CreateElement(METADATA);
                annotation.AppendChild(metadata);

                if (species.isSetCombaseCode())
                {
                    XmlElement combaseCodeNode = doc.CreateElement(SOURCE_TAG);
                    combaseCodeNode.InnerText = species.getCombaseCode();
                    metadata.AppendChild(combaseCodeNode);
                }

                if (species.isSetDetail())
                {
                    XmlElement detailNode = doc.CreateElement(DETAIL_TAG);
                    detailNode.InnerText = species.getDetail();
                    metadata.AppendChild(detailNode);
                }

                if (species.isSetDescription())
                {
                    XmlElement descriptionNode = doc.CreateElement(DESCRIPTION_TAG);
                    descriptionNode.InnerText = species.getDescription();
                    metadata.AppendChild(descriptionNode);
                }
            }
        }

        public SpeciesNuMLNode(XmlElement element)
        {
            this.element = element;
        }

        public PMFSpecies toPMFSpecies()
        {
            string id = element.GetAttribute(ID_ATTR);
            string name = element.GetAttribute(NAME_ATTR);
            string compartment = element.GetAttribute(COMPARTMENT_ATTR);
            string substanceUnits = element.GetAttribute(SUBSTANCE_UNITS_ATTR);
            PMFSpecies species = SBMLFactory.createPMFSpecies(compartment, id, name, substanceUnits);

            XmlElement annotationNode = (XmlElement)element.SelectSingleNode(ANNOTATION);
            if (annotationNode != null)
            {
                // metadata
                XmlElement metadataNode = (XmlElement)annotationNode.SelectSingleNode(METADATA);

                // combase code
                XmlElement combaseCodeNode = (XmlElement)metadataNode.SelectSingleNode(SOURCE_TAG);
                if (combaseCodeNode != null)
                {
                    species.setCombaseCode(combaseCodeNode.InnerText);
                }

                // detail 
                XmlElement detailNode = (XmlElement)metadataNode.SelectSingleNode(DETAIL_TAG);
                if (detailNode != null)
                {
                    species.setDetail(detailNode.InnerText);
                }

                // description
                XmlElement descriptionNode = (XmlElement)metadataNode.SelectSingleNode(DESCRIPTION_TAG);
                if (descriptionNode != null)
                {
                    species.setDescription(descriptionNode.InnerText);
                }
            }

            return species;
        }
    }

    public enum DataType { STRING, FLOAT, DOUBLE, INTEGER };

    // NuMLDocument
    // NuMLReader
    // NuMLWriter
    // ResultComponent

    public class ResultComponent
    {
        static string ELEMENT_NAME = "resultComponent";  // TODO: NuMLTags
        private static string ANNOTATION = "annotation";  // TODO: NuMLTags
        private static string METADATA = "metadata";  // TODO: NuMLTags

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

        // Constructors ...
        public ResultComponent() { }

        public ResultComponent(XmlElement element)
        {
            XmlElement annotationNode = (XmlElement)element.SelectSingleNode(ANNOTATION);
            XmlElement metadataNode = (XmlElement)annotationNode.SelectSingleNode(METADATA);

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
            XmlNodeList refNodes = metadataNode.GetElementsByTagName(ReferenceNuMLNode.TAG);
            references = new List<Reference>(refNodes.Count);
            for (int i = 0; i < refNodes.Count; i++)
            {
                XmlElement refElement = (XmlElement)refNodes.Item(i);
                references.Add(new ReferenceNuMLNode(refElement).ToReference());
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
    }

    public class TimeOntology
    {
        private static string ANNOTATION = "annotation";  // TODO: NuMLTags

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
            XmlElement annotationNode = (XmlElement)element.SelectSingleNode(ANNOTATION);
            XmlElement unitNode = (XmlElement)annotationNode.SelectSingleNode(NuMLTags.UNIT_DEFINITION);
            unitDefinition = new UnitDefinitionNuMLNode(unitNode).toPMFUnitDefinition();
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

            XmlElement annotation = doc.CreateElement(ANNOTATION);
            element.AppendChild(annotation);

            annotation.AppendChild(new UnitDefinitionNuMLNode(unitDefinition, doc).node);

            return element;
        }
    }

    public class Tuple
    {
        AtomicValue concValue;
        AtomicValue timeValue;

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

        public AtomicValue getConcValue() { return concValue; }
        public AtomicValue getTimeValue() { return timeValue; }

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
        AtomicDescription concDesc;
        AtomicDescription timeDesc;

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

        public AtomicDescription getConcentrationDescription() { return concDesc; }
        public AtomicDescription getTimeDescription() { return timeDesc; }

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
        private static string TRASNFORMATION_TAG = "pmf:transformation";
        private static string ID_ATTR = "id";
        private static string NAME_ATTR = "name";

        XmlElement element;

        public UnitDefinitionNuMLNode(PMFUnitDefinition unitDefinition, XmlDocument doc)
        {
            element = doc.CreateElement(NuMLTags.UNIT_DEFINITION);
            element.SetAttribute(ID_ATTR, unitDefinition.getId());
            element.SetAttribute(NAME_ATTR, unitDefinition.getName());

            // Adds transformation name
            if (unitDefinition.isSetTransformationName())
            {
                XmlElement transformationNode = doc.CreateElement(TRASNFORMATION_TAG);
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

        public PMFUnitDefinition toPMFUnitDefinition()
        {
            string id = element.GetAttribute(ID_ATTR);
            string name = element.GetAttribute(NAME_ATTR);

            XmlElement transformationNameNode = (XmlElement)element.SelectSingleNode(TRASNFORMATION_TAG);
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
        XmlElement element;

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
