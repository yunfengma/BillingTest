//----------------------------------------------------------------------------------------------------------
// <copyright file="BillingTestCommon.cs" company="Trupanion">
//    Copyright(c) 2019 - by Trupanion. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------------

namespace BillingTestCommon
{
    using global::BillingTestCommon.Models;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Mail;
    using System.Text;
    using System.Threading;
    using Trupanion.TruFoundation.Logging;

    public static class BillingTestCommon
    {
        public static ILog log = Logger.Log;

        public static string TestResutlsPath = string.Empty;
        public static string currentDir = Environment.CurrentDirectory;
        public static string testResultsFolder = Path.Combine(currentDir, @"TestResults");


        public static string RunTestMethod(string container, TestCategoryTypes category, out StringBuilder sbRet, out string emailMsg,
            string exResultName = "", int timeout = -1, bool SaveResult = true, bool email = true)
        {
            StringBuilder emailmsg = new StringBuilder();
            string str = string.Empty, cmd = string.Empty, arg = string.Empty, sResultOutput = string.Empty;
            string trxFile = Path.Combine(currentDir, $"TestResults\\{BillingTestCommonSettings.Default.TestResultTempFilePrefix}.trx");
            string htmlFile = Path.Combine(currentDir, $"TestResults\\{BillingTestCommonSettings.Default.TestResultTempFilePrefix}.trx.html");
            string trx2html = Path.Combine(currentDir, @"TestResults\" + BillingTestCommonSettings.Default.TrxToHtmlTool);
            string local = Environment.CurrentDirectory;                                                  // keep a local copy of the test result files
            string containerName = container.Substring(0, container.IndexOf('.'));

            sbRet = new StringBuilder();

            // check test containers
            if (string.IsNullOrEmpty(container))
            {
                str = "Empty test container!";
                emailmsg.AppendLine($"<p><font color=red>&#160;&#160;&#160;&#160;&#160;&#160;&#160;{str}</font></p>");
                sbRet.AppendLine(str);
                emailMsg = emailmsg.ToString();
                return email ? emailmsg.ToString() : sbRet.ToString();
            }

            // result file prefix only using the first container
            string rfileprefix = container.Split('\\')[container.Split('\\').Length - 1];
            string testsuitefix = rfileprefix.Split('.')[0];

            // run test
            try
            {
                // delete result trx file
                DeleteTestResultsFiles();

                // add containers in arg
                cmd = Path.Combine(GetVSIDSFolder(), "Common7\\IDE\\CommonExtensions\\Microsoft\\TestWindow");
                cmd = $"\"{Path.Combine(cmd, "vstest.console.exe")}\"";
                arg += $" {container} /InIsolation";

                // add category in arg if needed
                if (category != TestCategoryTypes.Undefined && category != TestCategoryTypes.All)
                {
                    arg += $" /testCaseFilter:\"TestCategory={category}\"";
                }

                // add result in arg
                arg += $" /Logger:trx";

                // run test
                sbRet.AppendLine($"Executing - {cmd} {arg}");
                sResultOutput = ExecuteCommandCatchOutput(cmd, arg);

                sbRet.AppendLine("\t\t" + sResultOutput);
                if (sResultOutput.ToLower().Contains("exitcode = 0"))
                {
                    emailmsg.AppendLine($"<b><font color=green>&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;PASSED</font></b><br></br>");
                }

                // convert trx to html
                trxFile = GetTestResultFile(testsuitefix);
                //bool bTrxFile = WaitForFileBeenCreated(trxFile);
                if (!string.IsNullOrEmpty(trxFile))
                {
                    sbRet.AppendLine("\ttrx file was created, convert to html format.");
                    str = ExecuteCommand($"\"{trx2html}\"", $"\"{trxFile}\"");
                    sbRet.AppendLine("\t\t" + str);
                    htmlFile = $"{trxFile}.html";
                }
                else
                {
                    sbRet.AppendLine($"\ttrx file was not created.");
                    log.Error($"\ttrx file was not created.");
                    SaveResult = false;
                    emailmsg.AppendLine($"<p>&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;Result html may not created, check trx file or log file for details</p>");
                }

                // save test result file
                if (SaveResult)
                {
                    string target = string.IsNullOrEmpty(TestResutlsPath) ? SetTestResulsFolder() : TestResutlsPath;          // test result folder on server

                    try
                    {
                        sbRet.AppendLine("\tCopy trx and html files to result folder.");
                        // copy trx file
                        str = ExecuteCommandCatchOutput("cmd", $"/C copy \"{trxFile}\" \"{local}\" /Y /Z");
                        str = ExecuteCommandCatchOutput("cmd", $"/C copy \"{trxFile}\" \"{target}\" /Y /Z");
                        sbRet.AppendLine("\t\t" + str);
                        // remove some useless lines
                        //RemoveHTMLViewer(htmlFile);
                        // copy html file
                        str = ExecuteCommand("cmd", $"/C copy \"{htmlFile}\" \"{local}\" /Y /Z");
                        str = ExecuteCommand("cmd", $"/C copy \"{htmlFile}\" \"{target}\" /Y /Z");
                        sbRet.AppendLine("\t\t" + str);
                        // email msg
                        str = $"{target}\\{Path.GetFileNameWithoutExtension(htmlFile)}.html";
                        if (sResultOutput.ToLower().Contains("exitcode = 0"))
                        {
                            emailmsg.AppendLine($"<p><font color=light blue>&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;Please check details <a href=\"{str}\">here</a></font></p>");
                        }
                        else
                        {
                            emailmsg.AppendLine($"<p><font color=red>&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;Please check details <a href=\"{str}\">here</a></font></p>");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        sbRet.AppendLine("\t\tException - " + ex.ToString());
                        throw new System.Exception("faied to copy result.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                sbRet.AppendLine("\t\tException - " + ex.ToString());
                emailmsg.AppendLine($"<b><font color=red>&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;&#160;Exception: {ex}</font></b><br></br>");
            }
            emailMsg = emailmsg.ToString();
            return email ? emailmsg.ToString() : sbRet.ToString();
        }


        public static string GetVSIDSFolder()
        {
            string ide = string.Empty;
            string pFile = Environment.GetEnvironmentVariable("ProgramFiles");
            if (8 == IntPtr.Size || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                pFile = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }
            ide = Path.Combine(pFile, BillingTestCommonSettings.Default.VSIDEFolder);
            return ide;
        }


        // executing cmd - to prevent the output redirection not responds to caller
        public static string ExecuteCommandCatchOutput(string exe, string args, bool waitforexit = true, int timeout = 300)
        {
            StringBuilder sbRet = new StringBuilder();
            int ExitCode = -1;                                      // default return, failed
            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();

            try
            {
                using (Process proc = new Process())
                {
                    // asynchronously with outputs
                    using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                    {
                        proc.OutputDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                try
                                {
                                    outputWaitHandle.Set();
                                }
                                catch { }
                            }
                            else
                            {
                                output.AppendLine(e.Data);
                            }
                        };
                        proc.ErrorDataReceived += (sender, e) =>
                        {
                            if (e.Data == null)
                            {
                                try
                                {
                                    errorWaitHandle.Set();
                                }
                                catch { }
                            }
                            else
                            {
                                error.AppendLine(e.Data);
                            }
                        };

                        ProcessStartInfo pInfo = new ProcessStartInfo(exe, args) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, WindowStyle = ProcessWindowStyle.Hidden };
                        proc.StartInfo = pInfo;
                        proc.EnableRaisingEvents = true;

                        // log / prompt
                        string str = $"ExecuteCommand() - {exe} {args}";
                        sbRet.AppendLine(str);

                        // start process
                        proc.Start();
                        proc.BeginOutputReadLine();
                        proc.BeginErrorReadLine();

                        // wait for exit
                        if (waitforexit) { proc.WaitForExit(); }

                        // get exist code and logging
                        ExitCode = proc.ExitCode;
                        sbRet.AppendLine($"ExitCode = {ExitCode}");
                        sbRet.AppendLine(ExitCode == 0 ? output.ToString() : error.ToString());
                    }
                }
            }
            catch (System.Exception ex)
            {
                sbRet.AppendLine($"Failed on executing cmdline of {exe} {args} with exception of {ex}");
                log.Error(ex.Message);
            }
            return sbRet.ToString();
        }


        // run cmdline exe with arguments
        // wait: -1 = wait for exit; ohters in second
        public static string ExecuteCommand(string exe, string args, int wait = -1)
        {
            StringBuilder sbRet = new StringBuilder();
            int ExitCode = -1;                                      // default return, failed

            try
            {
                using (Process proc = new Process())
                {
                    ProcessStartInfo pInfo = new ProcessStartInfo(exe, args) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, WindowStyle = ProcessWindowStyle.Hidden };
                    proc.StartInfo = pInfo;
                    proc.EnableRaisingEvents = true;
                    string str = $"ExecuteCommand() - {exe} {args}";
                    log.Info($"\t\t{str}");
                    sbRet.AppendLine(str);
                    Console.WriteLine(str);
                    proc.Start();
                    if (wait == -1)
                    {
                        proc.WaitForExit();
                        ExitCode = proc.ExitCode;
                        sbRet.AppendLine($"ExitCode = {ExitCode}");
                        log.Info($"\t\t\tExitCode = {ExitCode}");
                        if (ExitCode == 0)                          // no error
                        {
                            using (StreamReader sr = proc.StandardOutput)
                            {
                                sbRet.AppendLine(sr.ReadToEnd());
                            }
                        }
                        else                                        // get error message
                        {
                            using (StreamReader sr = proc.StandardError)
                            {
                                sbRet.AppendLine(sr.ReadToEnd());
                            }
                        }
                    }
                    else
                    {
                        proc.WaitForExit(wait * 1000);
                        //sbRet.AppendLine("ExitCode = -2");                              // unavailable
                    }
                }
            }
            catch (System.Exception ex)
            {
                string str = $"Failed on executing cmdline of {exe} {args} with exception of {ex}";
                log.Error($"{str}");
                sbRet.AppendLine(str);
            }
            return sbRet.ToString();
        }


        public static string SetTestResulsFolder(string machine = "")
        {
            StringBuilder sbRet = new StringBuilder();
            string folder = BillingTestCommonSettings.Default.TestResultFolder;
            string str = string.IsNullOrEmpty(BillingTestCommonSettings.Default.TestResultMachine) ? "c:\\" : "\\\\" + BillingTestCommonSettings.Default.TestResultMachine;
            string sFolder = Path.Combine(str, BillingTestCommonSettings.Default.TestResultFolder);

            try
            {
                if (!Directory.Exists(sFolder))
                {
                    Directory.CreateDirectory(sFolder);
                }
            }
            catch (System.Exception ex)
            {
                sbRet.AppendLine($"create the folder {folder} and run again. - {ex}");
            }

            // one sub folder per day and create when needed
            sFolder = Path.Combine(sFolder, DateTime.Now.ToString("MMddyyyy"));
            if (!Directory.Exists(sFolder))
            {
                try
                {
                    Directory.CreateDirectory(sFolder);
                    sbRet.AppendLine($"Result folder <{folder}> is created.");
                }
                catch
                {
                    sbRet.AppendLine($"Result folder <{folder}> is failed to be created.");
                }
            }
            else
            {
                sbRet.AppendLine($"Result folder <{folder}> already exists.");
            }
            return sFolder;
        }


        private static void RemoveHTMLViewer(string htm)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                // open trex.trx.htm
                using (StreamReader sr = new StreamReader(htm))
                {
                    while (true)
                    {
                        string line = sr.ReadLine();
                        if (line == null)
                        { break; }
                        if (line.Contains("The VSTS Test Results HTML Viewer"))
                        {
                            line = string.Empty;                // remove line
                        }
                        if (line.Contains("Test Class Detail"))
                        {
                            //sb.AppendLine("    </table><a href=\"#__top\">Back to top</a><br><h5></h5><a name=\"ID0ACD0FD\"></a><table border=\"0\">");
                        }
                        sb.AppendLine(line);
                    }
                }
                // update trex.trx.htm
                using (StreamWriter sw = new StreamWriter(htm))
                {
                    sw.Write(sb.ToString());
                }
            }
            catch
            { }
        }


        public static string SendEMail(string sFrom, string sTo, string sSubject, string sMailBody)
        {
            StringBuilder sbRet = new StringBuilder();
            MailMessage NewMail = new MailMessage();
            MailAddress MA = new MailAddress(sFrom);

            try
            {
                // email structure
                NewMail.From = MA;
                NewMail.To.Add(sTo);
                NewMail.Subject = sSubject;
                NewMail.IsBodyHtml = true;
                NewMail.Body = sMailBody;
                NewMail.Priority = MailPriority.Normal;
                // Init SmtpClient and send
                SmtpClient smtpClient = new SmtpClient("mail.trupanion.com", 25);                       // trupanion smtp and port
                System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("test.emails.auto", "");
                smtpClient.Credentials = credentials;
                smtpClient.Send(NewMail);
            }
            catch (System.Exception ex)
            {
                sbRet.AppendLine("SendMail() - " + ex);
                log.Error($"SendEMail() - {ex.Message}");
            }
            return sbRet.ToString();
        }


        public static void DeleteFilesWithType(string folder, string type)
        {
            FileInfo[] files = GetFilesByType(folder, type);
            foreach (FileInfo file in files)
            {
                try
                {
                    file.Attributes = FileAttributes.Normal;
                    File.Delete(file.FullName);
                }
                catch { }
            }
        }


        public static FileInfo[] GetFilesByType(string folder, string type)
        {
            DirectoryInfo di = new DirectoryInfo(folder);
            FileInfo[] files = di.GetFiles($"*.{type}");
            return files;
        }


        private static void DeleteTestResultsFiles()
        {
            DeleteFilesWithType(testResultsFolder, "*.trx");
            DeleteFilesWithType(testResultsFolder, "*.html");
        }


        private static string GetTestResultFile(string prefix)
        {
            string ret = string.Empty;
            FileInfo[] files = null;
            bool bFileExist = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            do
            {
                files = GetFilesByType(testResultsFolder, "*.trx");
                bFileExist = files.Length > 0;
            } while (!bFileExist && sw.ElapsedMilliseconds <= 18000);          // wait up to 3 minutes for the tex been generated

            if(files.Length > 0)
            {
                ret = $"{files[0].DirectoryName}\\{prefix}-{files[0].Name}";
                File.Copy(files[0].FullName, ret);
            }
            return ret;
        }


    }
}
