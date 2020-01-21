//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Trupanion">
//     Copyright(c) 2018-2018 by Trupanion. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Trupanion.Billing.Test
{
    using BillingTestCommon.Models;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Trupanion.TruFoundation.Logging;

    public class Program
    {
        public static ILog log = Logger.Log;

        public static StringBuilder emailMSG = new StringBuilder();                                         // global usages
        public static string EmailSubject = string.Empty;
        public static string EmailSubjectPostFix = string.Empty;
        public static TestCategoryTypes category = TestCategoryTypes.Undefined;
        public static string type = string.Empty;
        public static bool runApiTest { get; set; }
        public static bool runEnrollTest { get; set; }
        public static List<string> testCategories = new List<string>();


        [STAThread]
        static void Main(string[] args)
        {
            string str = string.Empty, userInput = string.Empty;
            string StarFolder = Environment.CurrentDirectory;

            try
            {
                // initial log
                log.Info("\r\n\r\n====================================================================================================");
                str = $"BillingTest has started on {Environment.MachineName} at {DateTime.Now}";
                log.Info($"{str}\r\n");
                emailMSG.AppendLine($"<b>{str}</b><br></br>");

                // check/setup input parameters
                userInput = ReadArgs(args);
                if(string.IsNullOrEmpty(userInput) || userInput.Contains("?") || userInput.Contains("help"))
                {
                    PrintHelp();
                }
                else
                {
                    SetExecConfig(userInput.ToLower());
                }

                emailMSG = new StringBuilder();
                EmailSubject = $"Billing Test: ";
                EmailSubjectPostFix = string.Empty;

                // email title post-fix
                EmailSubjectPostFix = $"{BillingTest.Default.TestEnvironment}";

                // initial the email title
                EmailSubject += $" {EmailSubjectPostFix}";

                // executing tests
                foreach(string test in testCategories)
                {
                    str = test.ToLower().Contains("api") ? "api" : "enrollment";
                    EmailSubject += $"......{str}";
                    str = $"    {test.Replace(".dll", "")} - {category}:";
                    emailMSG.AppendLine($"<b>&#160;&#160;&#160;&#160;&#160;&#160;{str}</b><br></br>");
                    log.Info($"{str}\r\n");
                    ExecTruExTest(test, type, category, out string emailMsg);
                    emailMSG.AppendLine(emailMsg);
                    log.Info($"\r\n");
                    emailMSG.AppendLine($"<b></b><br></br>");
                }
                EmailSubject += $"......Category={category}";
                str = $"\"\\\\{Path.Combine(BillingTestCommon.BillingTestCommonSettings.Default.TestResultMachine, BillingTestCommon.BillingTestCommonSettings.Default.TestResultFolder)}\\archive\"";
                emailMSG.AppendLine($"<p><font color=black>&#160;&#160;&#160;&#160;&#160;&#160;Passed test results may moved into <a href={str}>archive folder</a></font></p>");
            }
            catch (Exception ex)
            {
                str = "BillingTest Failed for: " + ex;
                log.Error(str);
                emailMSG.AppendLine($"<b><font color=red>{str}</font></b><br></br>");
            }
            finally
            {
                // logging end time
                emailMSG.AppendLine($"<b>BillingTest has finished on {Environment.MachineName} at {DateTime.Now}</b><br></br>");

                // email notice
                if (BillingTestCommon.BillingTestCommonSettings.Default.EmailTestResults)
                {
                    log.Info($"\tSending email to {BillingTestCommon.BillingTestCommonSettings.Default.EmailTestResultsTo} ...... ");
                    str = BillingTestCommon.BillingTestCommon.SendEMail(BillingTestCommon.BillingTestCommonSettings.Default.EmailTestResultsFrom, BillingTestCommon.BillingTestCommonSettings.Default.EmailTestResultsTo, EmailSubject, emailMSG.ToString());
                    if (str.ToLower().Contains("failed"))
                    {
                        log.Error(str);
                    }
                }

                log.Info(str);
                log.Info("====================================================================================================");
            }

            System.Environment.Exit(0);                         // somewhere, somehow, sometime the program not able to exit itself
        }

        private static void PrintHelp()
        {
            Console.WriteLine("need to run billing test? input like thest:");
            Console.WriteLine("");
            Console.WriteLine("            BillingTest [test suite (opetion)] [test category (requited]");
            Console.WriteLine("");
            Console.WriteLine("            where <test suite> = apitest, enrolltest, or default for both");
            Console.WriteLine("                  <test category> = Testing, BVT, or All");
            Console.WriteLine("");
            Console.WriteLine("            i.e. BillingTest Testing ApiTest");
            Console.WriteLine("                 BillingTest BVT");
            ConsoleKeyInfo inputs = Console.ReadKey();
            System.Environment.Exit(0);
        }

        private static string ReadArgs(string[] args)
        {
            string sRet = string.Empty;
            foreach (string s in args)
            {
                sRet += $"{s.ToLower()} ";
            }
            log.Info($"    input parameter: {sRet}\r\n");
            return sRet;
        }

        private static void SetExecConfig(string userInput)
        {
            if(userInput.Contains("all") || (!userInput.Contains("testing") && !userInput.Contains("bvt")))
            {
                category = TestCategoryTypes.All;
            }
            else if(userInput.Contains("testing"))
            {
                category = TestCategoryTypes.Testing;
            }
            else if (userInput.Contains("bvt"))
            {
                category = TestCategoryTypes.BVT;
            }
            

            runApiTest = userInput.Contains("api") ? true : false;
            runEnrollTest = userInput.Contains("enroll") ? true : false;

            // api test?
            if (runApiTest)
            {
                testCategories.Add("BillingApiTests.dll");
            }

            // enrollment tests?
            if (runEnrollTest)
            {
                testCategories.Add("EnrollmentTests.dll");
            }

        }

        public static string ExecTruExTest(string testDllName, string type, TestCategoryTypes category, out string emailMsg)
        {
            StringBuilder sbRet = new StringBuilder();
            string emailMessage = string.Empty;
            string str = BillingTestCommon.BillingTestCommon.RunTestMethod(testDllName, category, out sbRet, out emailMessage,
                exResultName: $"{type}_{BillingTest.Default.TestEnvironment}_{category}"
                );
            emailMsg = emailMessage;
            return sbRet.ToString();
        }


    }
}
