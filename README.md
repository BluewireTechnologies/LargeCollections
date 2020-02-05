# LargeCollections

Spills large collections to disk-based storage.

These days we would just use something like SQLite instead, but that was far less convenient back when this library was initially written.

## Future Plans

* The reference-counting implementation has been very useful and will likely be factored out and extended.
* The TableWriter, etc classes around bulk inserts and temporary tables have proven useful in many contexts. These will probably be maintained and extended to work with SQLite.
* The disk-based stuff is obsolete and will be removed once all our existing uses of it have been untangled.
