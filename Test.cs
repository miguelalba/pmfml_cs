using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    using System;
    using System.IO;
    using libsbmlcs;

    class Test
    {
        public static void Main(string[] args)
        {
            Model model = new Model(3, 1);

            Compartment compartment = new Compartment(3, 1);
            compartment.setId("cheese");
            compartment.setName("french-cheese");
            model.addCompartment(compartment);

            Species species = new Species(3, 1);
            species.setId("salmonella");
            species.setName("german-salmonella");
            model.addSpecies(species);

            SBMLDocument doc = new SBMLDocument(3, 1);
            doc.setModel(model);

            Console.Write(new SBMLWriter().writeSBMLToString(doc));
        }
    }
}
