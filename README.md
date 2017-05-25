# mentalcardgame

This is a C# implementation of the paper "A Toolbox for Mental Card Games"

This library allows to play a card game that is verifiable without a (trusted) third party. This is archived using zero-knowledge proves.

It is implemented as a Portable Library (.Net 4.5, Windows 8, Windows Phone 8.1).
## Operations
This is still not a Complete Implementation, but it supports the important operation like mask a card and shuffle.

### Key operations

*   Create Public/Private key pair (implemented)
*   Prove correctness of public key (not implemented)

### List of Card operations

*   Create open card (supported)
*   Mask (↬) open/covered card (supported)
*   Create private card (supported via creating an open card before using the mask operation)
*   Pickup/Uncover card (supported)
*   Create covered random Card (not implemented)

### List of Stack Operations

*   Permutation (supported)
*   Shuffle (supported with permutation performed by different players)
*   Prove of A ⊆ B (not implemented)
*   Prove of A ⊇ B (not implemented)
*   Prove of A ∩ B = ∅ (not implemented)
*   Prove for card C ∈ Stack A, C ∈ Stack B ⇔ A ∩ B ≠ ∅ (not implemented)
*   Cyclic shift of Stack (not implemented)
*   Insert Card at secrete position (not implemented)

### Other Operations

*   Prove A ↬ B or C ↬D (not implemented)
*   Entering and Leaving a game (not implemented)

## Misc 
For more Information you can find the paper [here](http://citeseerx.ist.psu.edu/viewdoc/summary?doi=10.1.1.29.6679).

A Nuget Package is also available [here](https://www.nuget.org/packages/MentalCardGame/1.0.0).

A C++ implementation can be found [here](http://www.nongnu.org/libtmcg/).
