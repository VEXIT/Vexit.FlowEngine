|              |                                                       |
| ------------ | ----------------------------------------------------- |
| Copyright    | © 2026 VEXIT ® , Tomorrow is today... , www.vexit.com |
| Author       | Vex Tatarevic                                         |
| Date Created | 2026-03-04                                            |
| Date Updated |                                                       |

# Vexit.FlowEngine - Creation Guide

## Create Workspace

Open terminal (GitBash on Windows) and type commands to create workspace

```bash
mkdir ~/dev
cd ~/dev
```
Now you should be inside `~/dev` folder.

> **NOTE:** From this point forward, all commands should be run from the workspace root folder !

## Create .NET Class Library Project

```bash
dotnet new classlib --name Vexit.FlowEngine # Create class library project
```

## Organize Project Structure

Move project files to src folder and create required directories:

```bash
# Create src directory first
mkdir -p Vexit.FlowEngine/src

# Move all files from Vexit.FlowEngine into Vexit.FlowEngine/src
mv Vexit.FlowEngine/* Vexit.FlowEngine/src/

# Create _docs directory
mkdir -p Vexit.FlowEngine/_docs
```

Your project structure should now be:
```
~/dev/Vexit.FlowEngine/
├── _docs/
└── src/
```

## Add NuGet Packages

```bash
dotnet add Vexit.FlowEngine/src package Microsoft.Extensions.Hosting  # hosting abstractions for IHostApplicationBuilder
```

- Verify package list:

  ```bash
  dotnet list Vexit.FlowEngine/src package
  ```

## Add Project References

```bash
dotnet add Vexit.FlowEngine/src reference Vexit/src
```

## Generate .gitignore file

```bash
dotnet new gitignore --output Vexit.FlowEngine/src
```

- You should see the .gitignore file generated in the Vexit.FlowEngine/src folder.



## Set Initial Version

- Set the initial version in **Vexit.FlowEngine.csproj**:

  ```xml
  <PropertyGroup>
    <Version>1.0.0</Version>  // <= Add this line
  </PropertyGroup>
  ```

  **OR** run this script to add it automatically:

  ```bash
  # Add version to Vexit.FlowEngine.csproj
  sed -i '/<\/PropertyGroup>/i\    <Version>1.0.0<\/Version>' Vexit.FlowEngine/src/Vexit.FlowEngine.csproj
  ```


## Create Test Project - Automatically

Run the script:

```bash
Vexit.Scripts/create-dotnet-unittests-project.sh Vexit.FlowEngine/src
```
- This will create Vexit.FlowEngine.Tests project in the Vexit.FlowEngine directory.

Move the test project to the tests folder:

```bash
# Move Vexit.FlowEngine.Tests to tests folder
mv Vexit.FlowEngine/Vexit.FlowEngine.Tests Vexit.FlowEngine/tests
```

Your project structure should now be:
```
~/dev/Vexit.FlowEngine/
├── _docs/
├── src/
└── tests/          # Contains Vexit.FlowEngine.Tests project files
```


## Create Test Project - Manually

### Create Project

We will create a separate test project for unit tests.

- Create the test project inside Vexit.FlowEngine directory:

  ```bash
  dotnet new classlib --output Vexit.FlowEngine/Vexit.FlowEngine.Tests
  ```

  > **NOTE:** We use `classlib` template (instead of xunit template) and add xUnit packages manually, instead of creating from `xunit` template (dotnet new xunit), to make sure xUnit version is the latest available. `Microsoft.NET.Test.Sdk` is required for .NET compatibility with Moq and other test dependencies.

- Delete the initial template file `Class1.cs`

  ```bash
  rm Vexit.FlowEngine/Vexit.FlowEngine.Tests/Class1.cs
  ```


### Organize Test Project Structure

Move the test project to the tests folder:

```bash
# Move Vexit.FlowEngine.Tests to tests folder
mv Vexit.FlowEngine/Vexit.FlowEngine.Tests Vexit.FlowEngine/tests
```

Your project structure should now be:
```
~/dev/Vexit.FlowEngine/
├── _docs/
├── src/
└── tests/          # Contains Vexit.FlowEngine.Tests project files
```

### Generate .gitignore file

```bash
dotnet new gitignore --output Vexit.FlowEngine/tests
```

- You should see the .gitignore file generated in the Vexit.FlowEngine/tests folder.

### Add NuGet Packages

```bash
dotnet add Vexit.FlowEngine/tests package Microsoft.NET.Test.Sdk                    # Test SDK required for .NET compatibility with test frameworks
dotnet add Vexit.FlowEngine/tests package xunit                                     # xUnit testing framework
dotnet add Vexit.FlowEngine/tests package xunit.runner.visualstudio                 # Test runner for Visual Studio and dotnet test
dotnet add Vexit.FlowEngine/tests package Moq                                       # Mocking framework for creating test doubles and isolating dependencies
dotnet add Vexit.FlowEngine/tests package FluentAssertions                          # Fluent API for more readable and expressive test assertions
```

- Verify project contains the added packages:

  ```bash
  dotnet list Vexit.FlowEngine/tests package
  ```


### Add Project References

```bash
dotnet add Vexit.FlowEngine/tests reference Vexit.FlowEngine/src
```

### Add Dummy Test

Create a basic test file to verify the test setup is working:

```bash
# Create dummy test file to verify test setup
cat > Vexit.FlowEngine/tests/DummyTest.cs << 'EOF'
using Xunit;

namespace Vexit.FlowEngine.Tests;

public class DummyTest
{
    [Fact]
    public void Should_Pass()
    {
        // Arrange
        var expected = true;

        // Act
        var actual = true;

        // Assert
        Assert.Equal(expected, actual);
    }
}
EOF
```

### Build Test Project

```bash
dotnet build Vexit.FlowEngine/tests
```

- You should see a successful build output.

### Run Tests

Below are a few different ways to run tests:

- Run tests - this command shows just the summary of the test execution

  ```bash
  dotnet test Vexit.FlowEngine/tests
  ```

- Run tests list - shows all test names

  ```bash
  dotnet test Vexit.FlowEngine/tests --list-tests
  ```

- Run tests with detailed console output

  ```bash
  dotnet test Vexit.FlowEngine/tests --logger "console;verbosity=normal"
  ```

- Run tests and generate an HTML report inside `TestResults/TestResults.html`

  ```bash
  dotnet test Vexit.FlowEngine/tests --logger "html;LogFileName=TestResults.html"
  ```


---

*© VEXIT ® 2026 | All rights reserved. | [www.vexit.com](https://www.vexit.com) | Tomorrow is today...®*