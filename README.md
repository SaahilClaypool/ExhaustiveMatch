# GenerateMatch

Generate a `.Match` method to switch over types with a known set of cases.
The idea is to create expression-style switch statements that ensure all conditions are handled.

The supported types are:

- enums: `Match` will take function parameters for each case of the enum
- enumeration classes: The source generator will infer each 
    - [feature is available in newer version of java](https://www.infoq.com/articles/java-sealed-classes/#:~:text=algebraic%20data%20types.\)-,Exhaustiveness,-Sealed%20classes%20like)
- Result and Option types: `Match` will handle unwrapping both the positive and negative cases

## Other ideas

TryGet Extensions returning option