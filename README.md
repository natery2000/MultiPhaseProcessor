# MultiPhaseProcessor

[![Build status](https://ci.appveyor.com/api/projects/status/79ukd0sot6iwvwyo?svg=true)](https://ci.appveyor.com/project/natery2000/multiphaseprocessor)

### Purpose

This repository is for the implementation of the MultiPhaseProcessor. It allows for chaining together a pipeline of functionality to process data in parallel.


### Use
Instantiate yourself a `Processor`:

    var processor = new Processor();
    
Add a head processor for the start of your pipeline:

    processor.WithHeadProcessor(<Function for this processor>);
  
Add zero or more middle processors:

    processor.WithProcessor(<Function for this processor>);
  
Add a tail processor for the end of your pipeline:

    processor.WithTailProcessor(<Function for this processor>);
  
Add your inputs:

    processor.AddWorkItems(<List of items>);
  
Initiate the processing:

    await processor.BeginAsync();

### Example
I need to take the strings "a", "b", "c" and repeat the string then add a "e" then print the values to the console.

    var processor = new Processor()
      .WithHeadProcessee((string s) => Task.Run(() => s + s))
      .WithProcessee((string s) => Task.Run(() => s + "e"))
      .WithTailProcessee((string s) => Task.Run(() => Console.WriteLine(s)));
      
    processor.AddWorkItems(new [] { "a", "b", "c" });
    
    await processor.BeginAsync();

### Contributing
This repository is open to PRs for issues.
