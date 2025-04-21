using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace DemoTest
{
    class Program
    {
        // Configuration
        private static readonly string CTFD_URL = "http://localhost:5173"; // Replace with your CTFd instance URL
        private static readonly string CHALLENGE_NAME = "Example Challenge"; // Replace with challenge name
        private static readonly string FLAG = "CTF{example_flag}"; // Replace with correct flag
        private static readonly string PASSWORD = "123456Aa@"; // Password for all users
        private static readonly int MAX_CONCURRENCY = 4; // Limit parallel instances to avoid overload

        static void Main(string[] args)
        {
            // Generate list of all 150 users
            var users = new List<(int Team, int User)>();
            for (int team = 1; team <= 50; team++)
            {
                for (int user = 1; user <= 3; user++)
                {
                    users.Add((team, user));
                }
            }

            // Run all users in parallel with limited concurrency
            Parallel.ForEach(users, new ParallelOptions { MaxDegreeOfParallelism = MAX_CONCURRENCY }, user =>
            {
                string username = $"Team{user.Team}User{user.User}";
                Console.WriteLine($"Processing user: {username}");

                IWebDriver driver = null;
                try
                {
                    driver = SetupDriver();
                    // Perform login
                    if (!Login(driver, username, PASSWORD))
                        return;

                    // Start challenge
                    if (!StartChallenge(driver))
                        return;

                    // Submit flag
                    SubmitFlag(driver);

                    // Log out
                    driver.Navigate().GoToUrl($"{CTFD_URL}/logout");
                    Thread.Sleep(1000); // Wait for logout
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {username}: {ex.Message}");
                }
                finally
                {
                    driver?.Quit(); // Ensure driver is closed
                }
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

        static bool StartChallenge(IWebDriver driver)
        {
            driver.Navigate().GoToUrl($"{CTFD_URL}/challenges");
            Thread.Sleep(1000); // Wait for challenges to load

            try
            {
                var challengeLink = driver.FindElement(By.XPath($"//a[contains(text(), '{CHALLENGE_NAME}')]"));
                challengeLink.Click();
                Thread.Sleep(1000); // Wait for challenge page to load
                Console.WriteLine($"Opened challenge: {CHALLENGE_NAME}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open challenge {CHALLENGE_NAME}: {ex.Message}");
                return false;
            }
        }

        static bool SubmitFlag(IWebDriver driver)
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
                    Console.WriteLine($"Flag submission successful for {CHALLENGE_NAME}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Flag submission failed: {ex.Message}");
                return false;
            }
        }
    }
}