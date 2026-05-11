using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DatesAndStuff.Web.Tests;

[TestFixture]
public class PersonPageTests
{
    private IWebDriver driver;
    private StringBuilder verificationErrors;
    private const string BaseURL = "http://localhost:5091";
    private bool acceptNextAlert = true;

    private Process? _blazorProcess;

    [OneTimeSetUp]
    public void StartBlazorServer()
    {
        var webProjectPath = Path.GetFullPath(Path.Combine(
            Assembly.GetExecutingAssembly().Location,
            "../../../../../../src/DatesAndStuff.Web/DatesAndStuff.Web.csproj"
            ));

        var webProjFolderPath = Path.GetDirectoryName(webProjectPath);

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            //Arguments = $"run --project \"{webProjectPath}\"",
            Arguments = "dotnet run --no-build",
            WorkingDirectory = webProjFolderPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        _blazorProcess = Process.Start(startInfo);

        // Wait for the app to become available
        var client = new HttpClient();
        var timeout = TimeSpan.FromSeconds(30);
        var start = DateTime.Now;

        while (DateTime.Now - start < timeout)
        {
            try
            {
                var result = client.GetAsync(BaseURL).Result;
                if (result.IsSuccessStatusCode)
                {
                    break;
                }
            }
            catch (Exception e)
            {
                Thread.Sleep(1000);
            }
        }
    }

    [OneTimeTearDown]
    public void StopBlazorServer()
    {
        if (_blazorProcess != null && !_blazorProcess.HasExited)
        {
            _blazorProcess.Kill(true);
            _blazorProcess.Dispose();
        }
    }

    [SetUp]
    public void SetupTest()
    {
        driver = new ChromeDriver();
        verificationErrors = new StringBuilder();
    }

    [TearDown]
    public void TeardownTest()
    {
        try
        {
            driver.Quit();
            driver.Dispose();
        }
        catch (Exception)
        {
            // Ignore errors if unable to close the browser
        }
        Assert.That(verificationErrors.ToString(), Is.EqualTo(""));
    }

    [Test]
    [TestCase("5")]
    [TestCase("12.5")]
    [TestCase("15")]
    public void Person_SalaryIncrease_ShouldIncrease(String inputprecent)
    {
        // Arrange
        driver.Navigate().GoToUrl(BaseURL);
        driver.FindElement(By.XPath("//*[@data-test='PersonPageNavigation']")).Click();

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

         var salaryLabel = wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@data-test='DisplayedSalary']")));
         var salaryBeforeSubmission = double.Parse(salaryLabel.Text);
        // double salaryBeforeSubmission = 5000.0; 
        
        var input = wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@data-test='SalaryIncreasePercentageInput']")));
        input.Clear();
        input.SendKeys(inputprecent);

        // Act
        var submitButton = wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@data-test='SalaryIncreaseSubmitButton']")));
        submitButton.Click();

        // Assert
        // wait.Until(ExpectedConditions.StalenessOf(salaryLabel));
        wait.Until(d =>
        {
            var currentText = d.FindElement(By.XPath("//*[@data-test='DisplayedSalary']")).Text;
            var currentVal = double.Parse(currentText);
            return currentVal != salaryBeforeSubmission;
        });
        salaryLabel = wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@data-test='DisplayedSalary']")));
        var salaryAfterSubmission = double.Parse(salaryLabel.Text);
        var salarycalculated = salaryBeforeSubmission + salaryBeforeSubmission *( double.Parse(inputprecent) / 100.0);
        salaryAfterSubmission.Should().BeApproximately(salarycalculated, 0.001);
    }

    [Test]
    [TestCase("-10")]
    [TestCase("-11")]
    [TestCase("-50")]
    public void Person_SalaryIncrease_BelowMinus10_ShouldShowErrorMessages(string inputPercent)
    {

        driver.Navigate().GoToUrl(BaseURL);
        driver.FindElement(By.XPath("//*[@data-test='PersonPageNavigation']")).Click();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

        var navButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@data-test='PersonPageNavigation']")));
        navButton.Click();

       
        var input = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@data-test='SalaryIncreasePercentageInput']")));

        input.Clear(); 
        input.SendKeys(inputPercent);

        // Act
        var submitButton = driver.FindElement(By.XPath("//*[@data-test='SalaryIncreaseSubmitButton']"));
        submitButton.Click();


        var errorMsg = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[contains(text(), '-10') and contains(text(), 'infinity')]")));
        errorMsg.Text.Should().NotBeNullOrWhiteSpace();

        var fieldError = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@data-test='SalaryIncreasePercentageInput']/following-sibling::div[@class='validation-message']")));
        fieldError.Text.Should().Contain("-10"); 
    }

    [Test]
    public void Wizz_air_test()
    {
        driver.Navigate().GoToUrl("https://www.wizzair.com/en-gb");

        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));


       
      //  ORIGIN

       var destinationInput = wait.Until(
           SeleniumExtras.WaitHelpers.ExpectedConditions
           .ElementToBeClickable(
               By.XPath("//*[@data-test='search-departure-station']"))
       );

        destinationInput.Click();
        //cookies
        try
        {
            var cookies = wait.Until(ExpectedConditions.ElementExists(By.XPath("//*[@id='accept']")));
            cookies.Click();
        }
        catch
        {
        }
        destinationInput.SendKeys("Budapest");

        // BUD
        var budapestOption = wait.Until(
            SeleniumExtras.WaitHelpers.ExpectedConditions
            .ElementToBeClickable(
                By.XPath("//*[@data-test='BUD']"))
        );

        budapestOption.Click();


        // DESTINATION

        var originInput = wait.Until(
            SeleniumExtras.WaitHelpers.ExpectedConditions
            .ElementToBeClickable(
                By.XPath("//*[@data-test='search-arrival-station']"))
        );

        originInput.Click();
        originInput.SendKeys("Bucharest");

        // OTP
        var otpOption = wait.Until(
            SeleniumExtras.WaitHelpers.ExpectedConditions
            .ElementToBeClickable(
                By.XPath("//*[@data-test='OTP']"))
        );

        otpOption.Click();

       


        DateTime nextWeek = DateTime.Now.AddDays(7);

        string formattedDate =
            $"{nextWeek.Year}-{nextWeek.Month:D2}-{nextWeek.Day:D2}";

        // DAY SELECT
        var dayButton = wait.Until(
            SeleniumExtras.WaitHelpers.ExpectedConditions
            .ElementToBeClickable(
                By.XPath($"//div[contains(@class,'id-{formattedDate}')]//span"))
        );

        dayButton.Click();
        dayButton.Click();

      

        // SEARCH
        var searchButton = wait.Until(
            SeleniumExtras.WaitHelpers.ExpectedConditions
            .ElementToBeClickable(
                By.XPath("//*[@data-test='flight-search-submit']"))
        );

        searchButton.Click();

        // RESULTS
        try
        {
            wait.Until(driver =>
                driver.FindElements(
                    By.XPath("//*[@data-test='flight-card']"))
                .Count > 0
            );
        }
        catch (Exception ex)
        {
            Assert.Fail("no flights found");
            return;
            //var notfound=wait.Until(ExpectedConditions.ElementIsVisible.XPath("//*[@data-test='no-flights-available']"));
        }

        var flights = driver.FindElements(
            By.XPath("//*[@data-test='flight-card']"));


        //bonusz
        int maxPrice = 200;

  
        var prices = driver.FindElements(
            By.XPath("//*[contains(@class,'price')]"));

        foreach (var price in prices)
        {
            string text = price.Text;

        
            string numbersOnly = new string(text.Where(char.IsDigit).ToArray());

            if (!string.IsNullOrEmpty(numbersOnly))
            {
                int currentPrice = int.Parse(numbersOnly);

                if (currentPrice < maxPrice)
                {
                    Screenshot screenshot =
                        ((ITakesScreenshot)driver).GetScreenshot();

                    string desktopPath =
                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                    string filePath =
                        Path.Combine(desktopPath, "kep.png");

                    screenshot.SaveAsFile(filePath);

                    Assert.Pass($"Cheap flight found: {currentPrice} RON");
                }
            }
        }

        // ASSERT
        Assert.That(flights.Count, Is.GreaterThanOrEqualTo(2),
            "Nincs legalább 2 járat Bucharest és Budapest között.");
    }

    private bool IsElementPresent(By by)
    {
        try
        {
            driver.FindElement(by);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    private bool IsAlertPresent()
    {
        try
        {
            driver.SwitchTo().Alert();
            return true;
        }
        catch (NoAlertPresentException)
        {
            return false;
        }
    }

    private string CloseAlertAndGetItsText()
    {
        try
        {
            IAlert alert = driver.SwitchTo().Alert();
            string alertText = alert.Text;
            if (acceptNextAlert)
            {
                alert.Accept();
            }
            else
            {
                alert.Dismiss();
            }
            return alertText;
        }
        finally
        {
            acceptNextAlert = true;
        }
    }
}