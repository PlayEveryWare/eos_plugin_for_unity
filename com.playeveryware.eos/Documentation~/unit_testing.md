# Unit Testing Guide for EOS Plugin for Unity

## Introduction to Unit Testing

Unit testing is a software testing technique in which individual components of the code are tested to ensure they work as expected. Each unit test should focus on a single function, class, or method and provide immediate feedback on whether that piece of code is functioning correctly. The goal is to catch and address bugs early in development, ensuring a stable and maintainable codebase.

### Benefits of Unit Testing
- **Improved code quality:** Unit tests ensure that code functions as expected before it is merged into the main codebase.
- **Early bug detection:** Tests provide an opportunity to catch and fix issues during development.
- **Facilitates refactoring:** With a solid unit test suite, developers can confidently refactor code knowing tests will highlight regressions.
- **Code coverage:** Tests help measure how much of the codebase is covered, ensuring critical functionality is always tested.

## Testing Frameworks in Use

Historically, this project has used both **NUnit** and the **Unity Test Framework** for unit testing. However, moving forward, we are transitioning to using **NUnit** exclusively. NUnit offers more flexibility and integration options for broader test case management and is better suited for the long-term needs of this project.

While there are still some existing tests using the Unity Test Framework, all new tests should be written using **NUnit**. This will ensure consistency across the test suite and streamline the development process.

### Why NUnit?

- **Flexibility:** NUnit supports a wide range of assertions, test case parameters, and test fixtures.
- **CI/CD Compatibility:** NUnit can be easily integrated into Continuous Integration and Deployment pipelines (planned for future).
- **Broad usage:** NUnit is a widely-adopted framework, making it familiar to many developers and well-supported across different IDEs.

## Scope of Unit Testing

Unit tests should be written for **scripts used in the sample scenes** and for the **core plugin code**. The following guidelines outline the scope:

### What is a priority to test:
- Core plugin functionality (e.g., helper classes, business logic, utilities, etc.).
- Scripts attached to game objects or components in the sample scenes (but not the scenes themselves).

### What is _not_ a priority to test:
- The **EOS SDK**, and it's C# wrapper. The behavior of the SDK and the C# wrapper provided with it is (currently) outside the scope of this project’s unit tests.
- **Sample scenes** (for now). Tests should focus on scripts, not scene setups or interactions between objects within the scenes.

## Writing Unit Tests

### Basic Structure of a Unit Test

Every unit test should follow these basic steps:
1. **Setup**: Initialize the objects or dependencies that will be tested.
2. **Act**: Execute the method or function you are testing.
3. **Assert**: Verify that the results are what you expect.

```csharp
[TestFixture]
public class ExampleTest
{
    [Test]
    public void Addition_ShouldReturnCorrectSum()
    {
        // Arrange
        int a = 5;
        int b = 3;
        
        // Act
        int result = a + b;
        
        // Assert
        Assert.AreEqual(8, result);
    }
}
```

### Coverage Requirements

Any **new functionality** added to the project should come with complete unit test coverage. The goal is to ensure that every feature introduced is tested thoroughly. 

When **refactoring** existing code, make sure that the unit tests are updated accordingly. Refactors should never lead to a reduction in test coverage.

### Handling Dependencies

While there is no current implementation for **mocking the EOS SDK**, this functionality is in development. Once mocking support is available, you will be able to write unit tests that mock dependencies on the EOS SDK.

For now, focus on isolating business logic and core plugin functionality in your tests.

## Moving Forward: Code Coverage Goals

As we transition fully to NUnit and increase our test coverage, the goal is to reach and maintain high code coverage across the entire project. To facilitate this, developers should:
- Write tests for all new code.
- Update tests when refactoring or modifying existing code.
- Ensure tests are well-structured, easy to read, and maintainable.

## Best Practices for Unit Tests

- **Small, isolated tests**: Each test should focus on a single function or method.
- **Meaningful names**: Test methods should clearly describe the scenario being tested.
- **Consistency**: Follow a consistent structure for arranging, acting, and asserting in your tests.
- **Avoid testing too much at once**: Unit tests should focus on small, testable components, not entire systems.
- **Repeatable and fast**: Tests should be able to run quickly and consistently without dependencies on external services or the environment.

## Naming Conventions for Unit Tests

To maintain clarity and consistency, unit test names should follow these conventions:

1. **Use Descriptive Names**: Test names should clearly describe the scenario being tested and the expected outcome. Follow the pattern:
   `MethodName_Scenario_ExpectedOutcome`
   
   - Example: `CalculateTotal_WithValidInputs_ReturnsCorrectSum()`

2. **PascalCase**: Use PascalCase for test method names, capitalizing each word.
   - Example: `Login_WithInvalidPassword_ThrowsException()`

3. **Group Tests by Class or Method**: Create test classes named after the class being tested, with `Tests` appended.
   - Example: `OrderProcessorTests`

4. **Arrange-Act-Assert**: Use comments or whitespace to clearly separate the three stages of each test.
   - Example:
   ```csharp
   [Test]
   public void GetPrice_WithDiscountApplied_ReturnsDiscountedPrice()
   {
       // Arrange
       var product = new Product(100);
       product.ApplyDiscount(10);

       // Act
       var result = product.GetPrice();

       // Assert
       Assert.AreEqual(90, result);
   }
   
5. **One Assertion Per Test**: Focus on testing one aspect of the method or functionality in each test. If multiple assertions are needed, ensure they are related to the same scenario.
   - Example: `SaveOrder_WhenOrderIsValid_CreatesNewOrderRecord()`

6. **Edge Cases**: Use specific names when testing edge cases or invalid inputs.
   - Example: `Withdraw_WithInsufficientFunds_ThrowsInsufficientFundsException()`

7. **Avoid Ambiguity**: Test names should avoid ambiguous phrases such as "Can" or "Does".
   - Example: `ProcessPayment_WithValidCreditCard_ReturnsSuccess()`

8. **Setup and Teardown Methods**: Use clearly named `[SetUp]` methods for common setup logic.
   - Example: `InitializeOrderProcessor()`

Following these conventions will ensure that tests are easy to read, maintain, and extend over time.

## Additional Reading and Resources

To further your understanding of unit testing and best practices, the following resources are highly recommended:
- [NUnit Documentation](https://docs.nunit.org/)
- [Unity Unit Testing Guide](https://learn.unity.com/tutorial/unit-testing)
- [Best Practices for Writing Unit Tests](https://martinfowler.com/bliki/TestPyramid.html)
  
## Conclusion

This guide provides a primer for adding unit tests to this Unity project. Following the outlined principles will ensure high test coverage, improved code quality, and a stable development environment.