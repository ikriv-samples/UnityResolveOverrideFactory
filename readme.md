# Unity Container: Register Factory with Overrides

This repository has there branches:
* **naive**: a  naive solution to the problem described below
* **simple**: a simple example of RegisterFactory with an override
* **master**: a better solution with nicer syntax

## The Problem

From time to time I run into a problem described in [this StackOverflow question](https://stackoverflow.com/questions/59143064/unity-tell-container-to-always-use-specific-implementation-of-an-interface-when "this StackOverflow question").

Suppose I have interface `IFoo` with default implementation `Foo` used throughout the application. Type `SpecialFooUser` depends on `IFoo` as well as  some other things, but I want to use `SpecialFoo` for this type, and not the standard `Foo`. 

I can do this with relative ease for a single resolution:

    container.Resolve<SpecialFooUser>(
        new DependencyOverride<IFoo>(container.Resolve<SpecialFoo>));

However, if I want Unity to **always** use this override when resolving `SpecialFooUser`, this is more difficult to achieve.

## A naive solution that works (branch 'naive')

I could register a factory that calls `new SpecialFooUser` and supplies all dependencies explicitly. This works, but explicitly repeating the list of dependencies just does not feel right. Not only it is verbose, it will have to be changed every time `SpecialFooUser` has a new dependency or depenency type changes. With refactoring and constant stream of new business requests, it happens more often than one might expect.

    container.RegisterFactory<SpecialFooUser>(
      c => new SpecialFooUser(c.Resolve<SpecialFoo>(), c.Resolve<IBar>()));

## A naive solution that does not work

If one attempts to combine `RegisterFactory` with `DependencyOverride`, it is tempting to do something like this:

    container.RegisterFactory<SpecialFooUser>(
      c => c.Resolve<SpecialFooUser>( // INFINITE RECURSION!!!
        new DependencyOverride<IFoo>(container.Resolve<SpecialFoo>));

This does not work, because `c.Resolve()` attempts to call the very factory we have just registered, causing infinite recursion and `StackOverflowException`.

## A better solution (branch 'simple')

We can work around the infinite loop by registering a named factory that oes default resolution, and calling that in `c.Resolve()`. This solution was partially inspired by Eugene Sadovoy's  comment on [this GitHub issue](https://github.com/unitycontainer/abstractions/issues/121 "this issue").

    container
      .RegisterType<SpecialFooUser>("stdreg")
      .RegisterFactory<SpecialFooUser>(
          c => c.Resolve<SpecialFooUser>("stdreg", 
            new DependencyOverride<IFoo>(c.Resolve<SpecialFoo>()))
        );

This, however, is still vebose and error prone.

## Even better solution (branch 'master')

We can reduce the clutter by implemeting a few extension methods and fluent-style classes. The registration the looks as follows:

    container.Register(
      new CustomFactory<SpecialFooUser>()
        .DependencyOverride<IFoo, SpecialFoo>());

The implementation of the machinery behind this succint intefrace takes about 100 lines of code, so it all depends on your taste and how often you have to do this kind of overrides. If this is a one-off the "better solution" above would probably be just fine.