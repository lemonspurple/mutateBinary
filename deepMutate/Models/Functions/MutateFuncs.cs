using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace deepMutate.Models.Data
{
    public class MutateFuncs
    {
        /* TODO
        1. Point mutations: 
                                GCA to 
                                GCC

        2. Indels
        Frameshift mutations:   ATG-CGT-ACG to 
                                ATC-GTA-CG

        In frame insert / delete:   ATG-CGT-ACG to
                                    ATG-GGC-CGT-ACG 

        3. Structural chromeosome mutations
        Duplications:   ABCDEF to 
                        ABCDEFDEF

        Deletion:       ABCDEF to
                        ABEF

        Inversion:      ABCDEF to
                        ABFEDC  
        Translokation:  ABCDEF is translocated to a completely
                        different part
    
        Evolutions: How often are the mutations applied
        */
    }
}
