### Running Mutation Tests with Stryker

To run mutation tests using the Stryker Mutator framework, follow these steps:

1. **Navigate to the test project**  
   Open a terminal and change directory to the test library:

   ```bash
   cd Domain.Tests
   ```

2. **Restore the required .NET tools**  
   Before running Stryker, make sure to restore the tools:

   ```bash
   dotnet tool restore
   ```

3. **Run Stryker**  
   Once the tools are restored, start mutation testing by running:

   ```bash
   dotnet stryker
   ```

4. **View the mutation report**  
   After the test run completes, Stryker will print a link in the terminal. Open this link in your browser to explore the full mutation testing report.
