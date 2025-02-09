namespace SteamPrefill
{
    /* TODO
     * Testing - Should invest some time into adding unit tests
     * Cleanup Resharper code issues, and github code issues.
     * Cleanup TODOs
     * Build - Fail build on trim warnings
     * Research - Determine if Polly could be used in this project
     * Docs - Make sure all terminal photos are the same width + height.  Retake if necessary
     */
    public static class Program
    {
        public static async Task<int> Main()
        {
            try
            {
                // Checking to see if the user double clicked the exe in Windows, and display a message on how to use the app
                OperatingSystemUtils.DetectDoubleClickOnWindows("SteamPrefill");

                var assembly = Assembly.GetExecutingAssembly();
                var informationVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

                var cliArgs = ParseHiddenFlags();
                var description = "Automatically fills a Lancache with games from Steam, so that subsequent downloads will be \n" +
                                  "  served from the Lancache, improving speeds and reducing load on your internet connection. \n" +
                                  "\n" +
                                  "  Start by selecting apps for prefill with the 'select-apps' command, then start the prefill using 'prefill'";
                return await new CliApplicationBuilder()
                             .AddCommandsFromThisAssembly()
                             .SetTitle("SteamPrefill")
                             .SetExecutableName($"SteamPrefill{(OperatingSystem.IsWindows() ? ".exe" : "")}")
                             .SetDescription(description)
                             .SetVersion($"v{informationVersion}")
                             .Build()
                             .RunAsync(cliArgs);
            }
            //TODO dedupe this throughout the codebase
            catch (TimeoutException e)
            {
                AnsiConsole.Console.MarkupLine("\n");
                if (e.StackTrace.Contains(nameof(UserAccountStore.GetUsernameAsync)))
                {
                    AnsiConsole.Console.MarkupLine(Red("Timed out while waiting for username entry"));
                }
                if (e.StackTrace.Contains(nameof(SpectreConsoleExtensions.ReadPasswordAsync)))
                {
                    AnsiConsole.Console.MarkupLine(Red("Timed out while waiting for password entry"));
                }
                AnsiConsole.Console.WriteException(e, ExceptionFormats.ShortenPaths);
            }
            catch (TaskCanceledException e)
            {
                if (e.StackTrace.Contains(nameof(AppInfoHandler.RetrieveAppMetadataAsync)))
                {
                    AnsiConsole.Console.MarkupLine(Red("Unable to load latest App metadata! An unexpected error occurred! \n" +
                                                       "This could possibly be due to transient errors with the Steam network. \n" +
                                                       "Try again in a few minutes."));

                    FileLogger.Log("Unable to load latest App metadata! An unexpected error occurred!");
                    FileLogger.Log(e.ToString());
                }
                else
                {
                    AnsiConsole.Console.WriteException(e, ExceptionFormats.ShortenPaths);
                }
            }
            catch (Exception e)
            {
                AnsiConsole.Console.WriteException(e, ExceptionFormats.ShortenPaths);
            }

            return 0;
        }

        /// <summary>
        /// Adds hidden flags that may be useful for debugging/development, but shouldn't be displayed to users in the help text
        /// </summary>
        /// <returns></returns>
        public static List<string> ParseHiddenFlags()
        {
            // Have to skip the first argument, since its the path to the executable
            var args = Environment.GetCommandLineArgs().Skip(1).ToList();

            if (args.Any(e => e.Contains("--debug")))
            {
                AppConfig.EnableSteamKitDebugLogs = true;
                args.Remove("--debug");
            }

            return args;
        }

        public static class OperatingSystem
        {
            public static bool IsWindows() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }
    }
}