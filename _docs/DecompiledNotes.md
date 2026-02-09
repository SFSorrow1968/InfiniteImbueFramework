# Decompiled ThunderRoad Notes

Key points from ThunderRoad.dll used by this framework:

- Imbue holds a single SpellCastCharge instance and exposes Transfer and SetEnergyInstant for loading spells and setting energy.
- Imbue energy drains through internal update logic, so the framework restores energy on an interval to keep imbues full.
- Item maintains a list of Imbue components for its collider groups; the module targets those entries.
