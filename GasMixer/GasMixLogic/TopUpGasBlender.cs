/*
 *    Adapted From https://github.com/atdotde/realblender
 *    Converted to C# by David Butt 2022-10-07
 * 
*/
using System.Text;
using GasMixLogic.Models;

namespace GasMixLogic;

public class GasCalculator
{
    private readonly GasMix _startingMix; //pi
    private readonly GasMix _targetMix;
    private readonly GasMix _firstTopUpMix;
    private readonly GasMix _secondTopUpMix;
    private readonly GasMix _finalTopUpMix;
    private readonly bool _isPsi;

    private readonly double[] _o2Coefficients = new[]
    {
        -7.18092073703e-04,
        +2.81852572808e-06,
        -1.50290620492e-09
    };

    private readonly double[] _n2Coefficients = new[]
    {
        -2.19260353292e-04,
        +2.92844845532e-06,
        -2.07613482075e-09
    };

    private readonly double[] _heCoefficients = new[]
    {
        +4.87320026468e-04,
        -8.83632921053e-08,
        +5.33304543646e-11
    };

    public GasCalculator(GasMix startingMix, GasMix targetMix, GasMix firstTopUpMix, GasMix secondTopUpMix,
        GasMix finalTopUpMix, bool isPsi = false)
    {
        _isPsi = isPsi;

        if (isPsi)
        {
            startingMix.Pressure = ConvertFromPsiToBar(startingMix.Pressure);
            targetMix.Pressure = ConvertFromPsiToBar(targetMix.Pressure);
            firstTopUpMix.Pressure = ConvertFromPsiToBar(firstTopUpMix.Pressure);
            if (secondTopUpMix != null)
                secondTopUpMix.Pressure = secondTopUpMix != null ? ConvertFromPsiToBar(secondTopUpMix.Pressure) : 0;
            finalTopUpMix.Pressure = ConvertFromPsiToBar(finalTopUpMix.Pressure);
            _startingMix = startingMix;
            _targetMix = targetMix;
            _firstTopUpMix = firstTopUpMix;
            _secondTopUpMix = secondTopUpMix;
            _finalTopUpMix = finalTopUpMix;
        }
        else
        {
            _startingMix = startingMix;
            _targetMix = targetMix;
            _firstTopUpMix = firstTopUpMix;
            _secondTopUpMix = secondTopUpMix;
            _finalTopUpMix = finalTopUpMix;
        }
    }

    private double FractionOfOxygen(GasMix mix)
    {
        return mix.Oxygen / 100;
    }

    private double FractionOfHelium(GasMix mix)
    {
        return mix.Helium / 100;
    }

    private double FractionOfNitrogen(GasMix mix)
    {
        var O2AndHe = mix.Oxygen + mix.Helium;
        return (100 - O2AndHe) / 100;
    }

    private double Virial(double pressure, double[] coefficients)
    {
        return coefficients[0] * pressure + coefficients[1] * pressure * pressure +
               coefficients[2] * pressure * pressure * pressure;
    }

    private double zfactor(double pressure, GasMix gasMix)
    {
        return 1 + FractionOfOxygen(gasMix) * Virial(pressure, _o2Coefficients) +
               FractionOfHelium(gasMix) * Virial(pressure, _heCoefficients) +
               FractionOfNitrogen(gasMix) * Virial(pressure, _n2Coefficients);
    }

    private double NormalVolumeFactor(double pressure, GasMix gasMix)
    {
        return pressure * zfactor(1, gasMix) / zfactor(pressure, gasMix);
    }

    private double FindPressure(GasMix mix, double volume)
    {
        var pressure = Math.Round(volume, 6);
        volume = Math.Round(volume, 6);
        var calc = Math.Abs(zfactor(1, mix) * pressure - zfactor(pressure, mix) + volume);
        while (calc > 0.000001)
        {
            pressure = Math.Round(volume * zfactor(pressure, mix) / zfactor(1, mix), 6);
            var zfac1 = zfactor(1, mix);
            var zfac2 = zfactor(pressure, mix);
            calc = Math.Round((zfac1 * pressure) - (zfac2 + volume), 6);
        }

        return pressure;
    }

    private string GasName(GasMix mix)
    {
        if (FractionOfHelium(mix) > 0)
        {
            return $"Trimix {Math.Round(FractionOfOxygen(mix) * 100 + .5)}/{Math.Round(FractionOfHelium(mix) * 100 + .5)}";
        }
        else
        {
            if (mix.Oxygen == 21)
            {
                return "Air";
            }
            else
            {
                return $"EAN {Math.Round(FractionOfOxygen(mix) * 100 + .5)}";
            }
        }
    }

    private double ConvertFromBarToPsi(double bar)
    {
        return bar * 14.5038;
    }

    private double ConvertFromPsiToBar(double psi)
    {
        return psi / 14.5038;
    }

    private double DegenerateGasCheck()
    {
        // var degenerate = false;
        var val = (FractionOfHelium(_finalTopUpMix) * FractionOfNitrogen(_secondTopUpMix) *
                   FractionOfOxygen(_firstTopUpMix))
                  - (FractionOfHelium(_secondTopUpMix) * FractionOfNitrogen(_finalTopUpMix) *
                     FractionOfOxygen(_firstTopUpMix))
                  - (FractionOfHelium(_finalTopUpMix) * FractionOfNitrogen(_firstTopUpMix) *
                     FractionOfOxygen(_secondTopUpMix))
                  + (FractionOfHelium(_firstTopUpMix) * FractionOfNitrogen(_finalTopUpMix) *
                     FractionOfOxygen(_secondTopUpMix))
                  + (FractionOfHelium(_secondTopUpMix) * FractionOfNitrogen(_firstTopUpMix) *
                     FractionOfOxygen(_finalTopUpMix))
                  - (FractionOfHelium(_firstTopUpMix) * FractionOfNitrogen(_secondTopUpMix) *
                     FractionOfOxygen(_finalTopUpMix));

        return val;
    }

    private bool SameGas(GasMix mix1, GasMix mix2)
    {
        //  if(Math.Round(FractionOfOxygen(mix1), 2) == Math.Round(FractionOfOxygen()))
        // if(Math.Abs(FractionOfOxygen(mix1)) == Math.Abs(FractionOfOxygen(mix2)))
        if (mix1.Oxygen == mix2.Oxygen)
        {
            return true;
        }

        return false;
    }

    public string CalculateFinalTriMixGasMix()
    {
        var det = DegenerateGasCheck();
        if (det > 0)
        {
            // return true;
        }
        else
        {
            // return false;
        }

        var initVol = NormalVolumeFactor(_startingMix.Pressure, _startingMix);
        var finalVol = NormalVolumeFactor(_targetMix.Pressure, _targetMix);

        var firstTopUp = (
                             (FractionOfNitrogen(_finalTopUpMix) * FractionOfOxygen(_secondTopUpMix) -
                              FractionOfNitrogen(_secondTopUpMix) * FractionOfOxygen(_finalTopUpMix))
                             * (FractionOfHelium(_targetMix) * finalVol - FractionOfHelium(_startingMix) * initVol)
                             + (FractionOfHelium(_secondTopUpMix) * FractionOfOxygen(_finalTopUpMix) -
                                FractionOfHelium(_finalTopUpMix) * FractionOfOxygen(_secondTopUpMix))
                             * (FractionOfNitrogen(_targetMix) * finalVol - FractionOfNitrogen(_startingMix) * initVol)
                             + (FractionOfHelium(_finalTopUpMix) * FractionOfNitrogen(_secondTopUpMix) -
                                FractionOfHelium(_secondTopUpMix) * FractionOfNitrogen(_finalTopUpMix))
                             * (FractionOfOxygen(_targetMix) * finalVol - FractionOfOxygen(_startingMix) * initVol)
                         )
                         / det;
        var secondTopUp = (
                              (FractionOfNitrogen(_firstTopUpMix) * FractionOfOxygen(_finalTopUpMix) -
                               FractionOfNitrogen(_finalTopUpMix) * FractionOfOxygen(_firstTopUpMix))
                              * (FractionOfHelium(_targetMix) * finalVol - FractionOfHelium(_startingMix) * initVol)
                              + (FractionOfHelium(_finalTopUpMix) * FractionOfOxygen(_firstTopUpMix) -
                                 FractionOfHelium(_firstTopUpMix) * FractionOfOxygen(_finalTopUpMix))
                              * (FractionOfNitrogen(_targetMix) * finalVol - FractionOfNitrogen(_startingMix) * initVol)
                              + (FractionOfHelium(_firstTopUpMix) * FractionOfNitrogen(_finalTopUpMix) -
                                 FractionOfHelium(_finalTopUpMix) * FractionOfNitrogen(_firstTopUpMix))
                              * (FractionOfOxygen(_targetMix) * finalVol - FractionOfOxygen(_startingMix) * initVol)
                          )
                          / det;


        var finalTopUp = (
                             (FractionOfNitrogen(_secondTopUpMix) * FractionOfOxygen(_firstTopUpMix) -
                              FractionOfNitrogen(_firstTopUpMix) * FractionOfOxygen(_secondTopUpMix))
                             * (FractionOfHelium(_targetMix) * finalVol - FractionOfHelium(_startingMix) * initVol)
                             + (FractionOfHelium(_firstTopUpMix) * FractionOfOxygen(_secondTopUpMix) -
                                FractionOfHelium(_secondTopUpMix) * FractionOfOxygen(_firstTopUpMix))
                             * (FractionOfNitrogen(_targetMix) * finalVol - FractionOfNitrogen(_startingMix) * initVol)
                             + (FractionOfHelium(_secondTopUpMix) * FractionOfNitrogen(_firstTopUpMix) -
                                FractionOfHelium(_firstTopUpMix) * FractionOfNitrogen(_secondTopUpMix))
                             * (FractionOfOxygen(_targetMix) * finalVol - FractionOfOxygen(_startingMix) * initVol)
                         )
                         / det;
        if (firstTopUp < 0 || secondTopUp < 0 || finalTopUp < 0)
        {
            return $"Impossible to blend {GasName(_targetMix)} with these gases!";
        }


        var oxygen =
            ((100 * (FractionOfOxygen(_startingMix) * initVol + FractionOfOxygen(_firstTopUpMix) * firstTopUp)) /
             (initVol + firstTopUp));
        var helium =
            ((100 * (FractionOfHelium(_startingMix) * initVol + FractionOfHelium(_firstTopUpMix) * firstTopUp)) /
             (initVol + firstTopUp));

        var mix1 = new TriMix(oxygen, helium);
        mix1.Pressure = FindPressure(mix1, initVol + firstTopUp);

        oxygen = ((100 * (FractionOfOxygen(_startingMix) * initVol + FractionOfOxygen(_firstTopUpMix) * firstTopUp +
                          FractionOfOxygen(_secondTopUpMix) * secondTopUp)) /
                  (initVol + firstTopUp + secondTopUp));

        helium = ((100 * (FractionOfHelium(_startingMix) * initVol + FractionOfHelium(_firstTopUpMix) * firstTopUp +
                          FractionOfHelium(_secondTopUpMix) * secondTopUp)) /
                  (initVol + firstTopUp + secondTopUp));

        var mix2 = new TriMix(oxygen, helium);
        mix2.Pressure = FindPressure(mix2, initVol + firstTopUp + secondTopUp);
        var sb = new StringBuilder();
        if (_isPsi)
        {
            sb.AppendLine($"Start with {ConvertFromBarToPsi(_startingMix.Pressure):F1} bar of {GasName(_startingMix)}");
            sb.AppendLine(
                $"Add {GasName(_firstTopUpMix)}  Until Pressure is {ConvertFromBarToPsi(mix1.Pressure)} to make the mix {GasName(mix1)}");
            sb.AppendLine(
                $"Then add {GasName(_secondTopUpMix)}  Until Pressure is {ConvertFromBarToPsi(mix2.Pressure)} to make the mix {GasName(mix2)}");
            sb.AppendLine(
                $"Finally top up with {GasName(_finalTopUpMix)} to {ConvertFromBarToPsi(_targetMix.Pressure)} and Final mix is {GasName(_targetMix)}");
        }
        else
        {
            sb.AppendLine($"Start with {_startingMix.Pressure:F1} bar of {GasName(_startingMix)}");
            sb.AppendLine(
                $"Add {GasName(_firstTopUpMix)}  Until Pressure is {mix1.Pressure} to make the mix {GasName(mix1)}");
            sb.AppendLine(
                $"Then add {GasName(_secondTopUpMix)}  Until Pressure is {mix2.Pressure} to make the mix {GasName(mix2)}");
            sb.AppendLine(
                $"Finally top up with {GasName(_finalTopUpMix)} to {_targetMix.Pressure} and Final mix is {GasName(_targetMix)}");
        }

        return sb.ToString();
    }

    public string CalculateNitroxGasMix()
    {
        if (SameGas(_firstTopUpMix, _finalTopUpMix))
        {
            return $"Cannot mix with identical gases!";
        }

        var initVol = NormalVolumeFactor(_startingMix.Pressure, _startingMix);
        var finalVol = NormalVolumeFactor(_targetMix.Pressure, _targetMix);

        var firstTopUp = (FractionOfOxygen(_finalTopUpMix) - FractionOfOxygen(_targetMix))
                         / (FractionOfOxygen(_finalTopUpMix) - FractionOfOxygen(_firstTopUpMix))
                         * finalVol
                         - (FractionOfOxygen(_finalTopUpMix) - FractionOfOxygen(_startingMix))
                         / (FractionOfOxygen(_finalTopUpMix) - FractionOfOxygen(_firstTopUpMix))
                         * initVol;
        var secondTopUp = (FractionOfOxygen(_firstTopUpMix) - FractionOfOxygen(_targetMix))
                          / (FractionOfOxygen(_firstTopUpMix) - FractionOfOxygen(_finalTopUpMix))
                          * finalVol
                          - (FractionOfOxygen(_firstTopUpMix) - FractionOfOxygen(_startingMix))
                          / (FractionOfOxygen(_firstTopUpMix) - FractionOfOxygen(_finalTopUpMix))
                          * initVol;

        if (firstTopUp <= 0)
        {
            return "Impossible to blend with these gasses!";
        }

        var oxygen =
            (100 * (FractionOfOxygen(_startingMix) * initVol + FractionOfOxygen(_firstTopUpMix) * firstTopUp)) /
            (initVol + firstTopUp);

        var mix1 = new Nitrox(oxygen);
        mix1.Pressure = FindPressure(mix1, initVol + firstTopUp);

        var sb = new StringBuilder();

        if (_isPsi)
        {
            sb.AppendLine($"Start with {ConvertFromBarToPsi(_startingMix.Pressure):F1} bar of {GasName(_startingMix)}");
            sb.AppendLine(
                $"Add {GasName(_firstTopUpMix)}  Until Pressure is {Math.Round(ConvertFromBarToPsi(mix1.Pressure), 1)} to make the mix {GasName(mix1)}");
            sb.AppendLine(
                $"Finally top up with {GasName(_finalTopUpMix)} to {ConvertFromBarToPsi(_targetMix.Pressure)} and Final mix is {GasName(_targetMix)}");
        }
        else
        {
            sb.AppendLine($"Start with {_startingMix.Pressure:F1} bar of {GasName(_startingMix)}");
            sb.AppendLine(
                $"Add {GasName(_firstTopUpMix)}  Until Pressure is {Math.Round(mix1.Pressure, 1)} to make the mix {GasName(mix1)}");
            sb.AppendLine(
                $"Finally top up with {GasName(_finalTopUpMix)} to {_targetMix.Pressure} and Final mix is {GasName(_targetMix)}");
        }

        return sb.ToString();
    }
}