using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace DemoTest
{
    class Program
    {
        // Configuration
        private static readonly string CTFD_URL = "http://localhost:5173"; // CTFd instance on port 5173
        private static readonly string FLAG = "CTF{example_flag}"; // Replace with correct flag
        private static readonly string PASSWORD = "123456Aa@"; // Password for all users
        private static readonly int MAX_CONCURRENCY = 4; // Limit parallel instances

        static void Main(string[] args)
        {
            // Generate list of all 150 users with their challenge URLs
            var users = new List<(int Team, int User, string ChallengeUrl)>();
            for (int team = 1; team <= 50; team++)
            {
                users.Add((team, 1, $"{CTFD_URL}/challenge/7")); // User1: challenge 7
                users.Add((team, 2, $"{CTFD_URL}/challenge/8")); // User2: challenge 8
                users.Add((team, 3, $"{CTFD_URL}/challenge/9")); // User3: challenge 9
            }

            // Run all users in parallel with limited concurrency
            Parallel.ForEach(users, new ParallelOptions { MaxDegreeOfParallelism = MAX_CONCURRENCY }, user =>
            {
                string username = $"Team{user.Team}User{user.User}";
                string challengeUrl = user.ChallengeUrl;
                Console.WriteLine($"Processing user: {username} for {challengeUrl}");

                IWebDriver driver = null;
                try
                {
                    driver = SetupDriver();
                    // Perform login
                    if (!Login(driver, username, PASSWORD))
                        return;

                    // Start challenge
                    if (!StartChallenge(driver, challengeUrl))
                        return;

                    // Submit flag
                    //SubmitFlag(driver, username);

                    // Log out
                    //driver.Navigate().GoToUrl($"{CTFD_URL}/logout");
                    //Thread.Sleep(1000); // Wait for logout
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {username}: {ex.Message}");
                }
                //finally
                //{
                //    driver?.Quit(); // Ensure driver is closed
                //}
            });
        }

        static IWebDriver SetupDriver()
        {
            var options = new ChromeOptions();
            // No --headless to show browser window
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            return new ChromeDriver(options);
        }

        static bool Login(IWebDriver driver, string username, string password)
        {
            driver.Navigate().GoToUrl($"{CTFD_URL}");
            Thread.Sleep(1000); // Wait for page to load

            try
            {
                var usernameField = driver.FindElement(By.Name("username"));
                var passwordField = driver.FindElement(By.Name("password"));
                var submitButton = driver.FindElement(By.XPath("//button[@type='submit']"));

                usernameField.SendKeys(username);
                passwordField.SendKeys(password);
                submitButton.Click();
                Thread.Sleep(1000); // Wait for login to process

                if (!driver.Url.Contains("login"))
                {
                    Console.WriteLine($"Login successful for {username}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Login failed for {username}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error for {username}: {ex.Message}");
                return false;
            }
        }

        static bool StartChallenge(IWebDriver driver, string challengeUrl)
        {
            driver.Navigate().GoToUrl(challengeUrl);
            Thread.Sleep(1000); // Wait for challenge page to load

            try
            {
                // Click the "Start Challenge" button
                var startButton = driver.FindElement(By.XPath("//button[contains(text(), 'Start Challenge')]"));
                startButton.Click();
                Thread.Sleep(1000); // Wait for button action to complete

                // Click the "here" link
                var hereLink = driver.FindElement(By.XPath("//a[contains(text(), 'here')]"));
                string originalWindow = driver.CurrentWindowHandle;
                hereLink.Click();
                Thread.Sleep(1000); // Wait for link action

                // Switch back to original window (since link opens in new tab)
                driver.SwitchTo().Window(originalWindow);

                // Verify flag input field is present
                //driver.FindElement(By.Id("flag"));
                Console.WriteLine($"Started challenge at {challengeUrl}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start challenge at {challengeUrl}: {ex.Message}");
                return false;
            }
        }

        static bool SubmitFlag(IWebDriver driver, string username)
        {
            try
            {
                var flagField = driver.FindElement(By.Id("flag"));
                var submitButton = driver.FindElement(By.XPath("//button[@type='submit']"));
                flagField.SendKeys(FLAG);
                submitButton.Click();
                Thread.Sleep(1000); // Wait for submission to process

                var successMessage = driver.FindElement(By.ClassName("alert-success"));
                if (successMessage != null)
                {
                    Console.WriteLine($"Flag submission successful for {username}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Flag submission failed for {username}: {ex.Message}");
                return false;
            }
        }
    }
}