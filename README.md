# MultiPhaseProcessor

[![Build status](https://ci.appveyor.com/api/projects/status/79ukd0sot6iwvwyo?svg=true)](https://ci.appveyor.com/project/natery2000/multiphaseprocessor)

### Purpose

This repository is for the implementation of the MultiPhaseProcessor. It allows for chaining together a pipeline of functionality to process data in parallel.


### Use
Instantiate yourself a `Processor`:

    var processor = new Processor();
  
Add one or more processors:

    processor.WithProcessor(<Function for this processor>);
    
Add your inputs:

    processor.AddWorkItems(<List of items>);
  
Initiate the processing:

    await processor.BeginAsync();

### Example
I need to take the strings "a", "b", "c" and repeat the string then add a "e" then print the values to the console.

    var processor = new Processor()
      .WithProcessee((string s) => Task.Run(() => s + s))
      .WithProcessee((string s) => Task.Run(() => s + "e"))
      .WithProcessee((string s) => Task.Run(() => { Console.WriteLine(s); return Task.FromResult(string.Empty);));
      
    processor.AddWorkItems(new [] { "a", "b", "c" });
    
    await processor.BeginAsync();

### Contributing
This repository is open to PRs for issues.
