# Line Extrusion (2D)

### Overview

The aim of this project is to create proper extruded surfaces from lines in two dimensions.

Straightforward implementations based on shifting points along the line normal directions can run into issues when the line's curvature is high, and the extruded surface begins to intersect on itself.

Visualization and implementation is in Unity.

### Extrusion Notes

The first step is to consider the set of all points exactly the extrusion distance away from the line. The extruded surface must fall within this space, as otherwise, the shortest distance from a point to the line will be either less than the extrusion distance (hence, the point is too close and within the surface) or larger (the point is too far and outside of the surface).

To create this surface for a given line, we break up the line into segments, and consider the extrusion of this now piecewise-defined line. When two line segments form a concave angle, their extruded segments will intersect. However, when they form a convex angle, there will be a circular arc of points at the extrusion distance to cover the gap between the extrusion of the two individual segments.

The next step is to remove any part of this surface that is closer than the extrusion distance to the line. This can happen when the line's curvature is high, or when different parts of the line come within the extrusion distance of each other.

Consider the closest distance field from the line. We know that the initially-extruded surface lies within the isosurface at the extrusion distance, with some parts lying at distances less than the extrusion distance. In most cases, we expect the closest distance field to be smooth, with the extrusion distance isosurface defining a continuous line (the regular extruded surface). However, in some cases, following along this extruded line will mean that it ends up leaving this isosurface, becoming closer to the original line than the extrusion distance. For this to happen, it must cross another section of the isosurface.

Thus, intersections of the initially-extruded surface with itself form the boundaries between subsections of that surface. we can then test each subsection to determine if it is indeed closer than the extrusion distance or not.

### Texture Mapping Notes

In normal extrusion situations, texture mapping is performed by assigning the u-parameter from arcdistance along the line, and the v-parameter the perpendicular distance from the line.

When the extruded contour intersects with itself, there is a discontinuity in the expected u-parameter from the aforementioned method.

To deal with these discontinuities, the u-parameters are altered. The most promising method is a Gaussian convolution arcdistance line integral of u-parameters along the extruded contour.

Finally, triangulation of the above results can yield shearing artifacts of the texture mapping. Instead, inverse bilinear interpolation is used to smoothly connect points on either side of the original line with the same u-parameter.

When extrusion is large enough that the extruded surface has an interior hole, there is no obvious aesthetic choice for how to perform texture mapping. As a result, no mapping is used.

### Improvements

- Any spatial partitioning data structure would provide performance improvements, particularly if the original line is segmented into small pieces.
- Data used for the inverse bilinear interpolation is provided to the shader as an array; instead providing this data in a texture may make more sense, depending on platform.
- An example spline is provided, but is not production-ready.

### Thanks

Catlike Coding (Jasper Flick) Curves and Splines tutorial contains a Unity Bezier Spline implementation used as a base for an example line to segment and extrude. See: http://catlikecoding.com/unity/tutorials/curves-and-splines/.

Triangle.NET code for contour polygonization is used in cases where the extruded surface intersects itself to create a hole inside the contour. See: https://triangle.codeplex.com/

### License
> Licensed under the [**MIT License**](https://en.wikipedia.org/wiki/MIT_License).
