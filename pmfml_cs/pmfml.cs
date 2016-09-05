using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pmfml_cs
{
    public enum ModelClass
    {
        UNKNOW = 0,
        GROWTH = 1,
        INACTIVATION = 2,
        SURVIVAL = 3,
        GROWTH_INACTIVATION = 4,
        INACTIVATION_SURVIVAL = 5,
        GROWTH_SURVIVAL = 6,
        GROWTH_INACTIVATION_SURVIVAL = 7,
        T = 8,
        PH = 9,
        AW = 10,
        T_PH = 11,
        T_AW = 12,
        PH_AW = 13,
        T_PH_AW = 14
    }

    public static class Extensions
    {
        public static string modelClassName(ModelClass mc)
        {
            switch (mc)
            {
                case ModelClass.UNKNOW: return "unknown";
                case ModelClass.GROWTH: return "growth";
                case ModelClass.INACTIVATION: return "inactivation";
                case ModelClass.SURVIVAL: return "survival";
                case ModelClass.GROWTH_INACTIVATION: return "growth/inactivation";
                case ModelClass.INACTIVATION_SURVIVAL: return "inactivation/survival";
                case ModelClass.GROWTH_SURVIVAL: return "growth/survival";
                case ModelClass.GROWTH_INACTIVATION_SURVIVAL: return "growth/inactivation/survival";
                case ModelClass.T: return "T";
                case ModelClass.PH: return "pH";
                case ModelClass.AW: return "aw";
                case ModelClass.T_PH: return "T/pH";
                case ModelClass.T_AW: return "T/aw";
                case ModelClass.PH_AW: return "pH/aw";
                case ModelClass.T_PH_AW: return "T/pH/aw";
                default: return "unknown";
            }
        }
    }

    public enum ModelType
    {
        EXPERIMENTAL_DATA,  // Experimental data
        PRIMARY_MODEL_WDATA,  // Primary models generated from data records
        PRIMARY_MODEL_WODATA, // Primary models generated without data records
        TWO_STEP_SECONDARY_MODEL,  // Secondary models generated with the classical two step fit approach
        ONE_STEP_SECONDARY_MODEL,  // Secondary models generated implicitely during 1-step fitting of tertiary models
        MANUAL_SECONDARY_MODEL,  // Manually generated secondary models
        TWO_STEP_TERTIARY_MODEL,  // Tertiary model generated with 2-step fit approach
        ONE_STEP_TERTIARY_MODEL,  // Tertiary model generated with 1-step fit approach
        MANUAL_TERTIARY_MODEL  // Tertiary models generated manually

    }

    public interface LiteratureSpecificationI
    {
        string getAuthor();
        string getYear();
        string getTitle();
        string getAbstract();
        string getJournal();
        string getVolume();
        string getIssue();
        string getPage();
        string getApproval();
        string getWebsite();
        string getType();
        string getComment();
    }

}