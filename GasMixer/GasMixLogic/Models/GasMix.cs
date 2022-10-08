namespace GasMixLogic.Models;

public class GasMix
{
    public double Pressure { get; set; }
    public double Helium { get; set; }
    public double Oxygen { get; set; }
    public double Nitrogen { get; set; }
}

public class Air : GasMix
{
    public Air()
    {
        Pressure = 0;
        Helium = 0;
        Nitrogen = 79;
        Oxygen = 21;
    }
}

public class Nitrox : GasMix
{
    public Nitrox(double o2)
    { 
        Pressure = 0;
        Nitrogen = 100 - o2;
        Helium = 0;
        Oxygen = o2;
    }
}
    
public class TriMix : GasMix
{
    public TriMix(double o2, double he)
    {
        Pressure = 0;
        Helium = he;
        Oxygen = o2;
        Nitrogen = 100 - (o2 + he);
    }
    
}