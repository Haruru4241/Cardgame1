using System;

[Flags]
public enum ZoneType
{
    None = 0,
    Rule = 1 << 0, // 1
    Deck = 1 << 1, // 2
    Discard = 1 << 2, // 4
    Exhaust = 1 << 3, // 8
    Hand = 1 << 4, // 16
    Destroy = 1 << 5, // 32
    Used = 1 << 6, // 64
    Shop = 1 << 7, // 128
}