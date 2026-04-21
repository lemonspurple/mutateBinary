using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mutateBinary.Models.Data
{
    public class MutateFuncs
    {
        /* TODO
        A 1. Point mutations: 
                                GCA to 
                                GCC

        B 2. Indels
        Frameshift mutations:   ATG-CGT-ACG to 
                                ATC-GTA-CG

        C In frame insert / delete:   ATG-CGT-ACG to
                                    ATG-GGC-CGT-ACG 

        3. Structural chromeosome mutations
        D Duplications:   ABCDEF to 
                        ABCDEFDEF

        E Deletion:       ABCDEF to
                        ABEF

        F Inversion:      ABCDEF to
                        ABFEDC  
        Translokation:  ABCDEF is translocated to a completely
                        different part
    
        Evolutions: How often are the mutations applied
        */
    }
}
