namespace mutateBinary.Models.Data
{
    public class MutateFuncs
    {
        float pointValue = default;
        float frameshiftValue = default;
        float frameInsertDeleteValue = default;
        float duplicationValue = default;
        float deletionValue = default; 
        float inversionValue = default;
        float translocationValue = default;
        double cyclesValue = default;
    

    public MutateFuncs(float _pointValue, float _frameshiftValue, float _frameInsertDeleteValue, float _duplicationValue, float _deletionValue, float _inversionValue, float _translocationValue, double _cyclesValue)
    {
        pointValue = _pointValue;
        frameshiftValue = _frameshiftValue;
        frameInsertDeleteValue = _frameInsertDeleteValue;
        duplicationValue = _duplicationValue;
        deletionValue = _deletionValue;
        inversionValue = _inversionValue;
        translocationValue = _translocationValue;
        cyclesValue = _cyclesValue;
    }

    public void printMutateValuesToDebug()
        {
            
        }


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
