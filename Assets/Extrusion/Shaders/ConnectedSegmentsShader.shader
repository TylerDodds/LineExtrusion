Shader "BabyDinoHerd/ConnectedSegmentsShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }

		Blend SrcAlpha OneMinusSrcAlpha

		LOD 100

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			
			#include "UnityCG.cginc"

			// Two-dimensional cross-product analogue.
			float Cross2D(in float2 vector1, in float2 vector2)
			{
				return vector1.x * vector2.y - vector1.y * vector2.x;
			}

			// Distance squared from a test point to the interior of the unit square.
			float DistanceSquaredToUnitSquare(in float2 vec)
			{
				float2 delta = max(0, max(-vec, vec - 1));
				return dot(delta, delta);
			}

			// Inverse bilinear interpolation fractions, with a-b and c-d interpolated by second fraction, and those results interpolated by the first fraction.
			// For our purposes we want the solution that will be closest to the unit square.
			// We expect to find an answer within the unit square always in the conditions this will be used within this shader.
			float2 InverseBilinearInterpolation(in float2 p, in float2 a, in float2 b, in float2 c, in float2 d)
			{
				const float epsilon = 1e-5;
				float2 alpha = c - a;
				float2 beta = b - a;
				float2 gamma = a - c + d - b;
				float2 delta = p - a;

				float q = Cross2D( beta, gamma );
				float r = Cross2D( beta, alpha ) + Cross2D( gamma, delta );
				float s = Cross2D( alpha, delta );

				float discriminant = r * r - 4.0 * q * s;
				if(discriminant < 0.0) 
				{
					return float2(-1.0, -1.0);
				}
				if(abs(q) < epsilon)
				{
					float vLinear = - s / r;
					float uLinear = (delta.x - beta.x * vLinear) / (alpha.x + gamma.x * vLinear);
					return float2(uLinear, vLinear);
				}
				discriminant = sqrt(discriminant);

				float v1 = (-r - discriminant) / (2.0 * q);
				float u1 = (delta.x - beta.x * v1) / (alpha.x + gamma.x * v1);
				float2 uv1 = float2(u1, v1);

				float v2 = (-r + discriminant) / (2.0 * q);
				float u2 = (delta.x - beta.x * v2) / (alpha.x + gamma.x * v2);
				float2 uv2 = float2(u2, v2);

				float uv1DistSqToUnitSquare = DistanceSquaredToUnitSquare(uv1);
				float uv2DistSqToUnitSquare = DistanceSquaredToUnitSquare(uv2);

				float2 uv = uv1DistSqToUnitSquare <= uv2DistSqToUnitSquare ? uv1 : uv2;

				return uv;
			}

			// Finds the uv of a point by performing inverse bilinear interpolation between four points, and taking the interpolated uv.
			float2 InverseBilinearUv(in float2 p, in float4 a, in float4 b, in float4 c, in float4 d)
			{
				float2 inverseBilinearFractions = InverseBilinearInterpolation(p, a.xy, b.xy, c.xy, d.xy);
				
				float4 abInterpolatedByV = b * inverseBilinearFractions.y + a * (1.0 - inverseBilinearFractions.y);
				float4 cdInterpolatedByV = d * inverseBilinearFractions.y + c * (1.0 - inverseBilinearFractions.y);
				float4 abToCdInterpolatedByU = cdInterpolatedByV * inverseBilinearFractions.x + abInterpolatedByV * (1.0 - inverseBilinearFractions.x);
				
				return abToCdInterpolatedByU.zw;
			}

			// Vertex shader data
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 segmentIndices : TEXCOORD3;
			};

			// Fragment shader data
			struct Interpolators
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 localxy : TEXCOORD2;
				float2 segmentIndices : TEXCOORD8;
			};

			//Texture samplers
			sampler2D _MainTex;
			float4 _MainTex_ST;

			//NB Usually Unity's maximum length of arrays passed to shaders is 1023. 
			//However, when including four of them, by experimentation the magic maximum number is 1022 ...
			//This will be platform-dependent, both in terms of shader compilation and linking. Further experimentation may be needed
			// Important! Keep this in sync with ConnectedSegmentsMaterialSet.
			static const uint _maxNumMonotonicChunkPoints = 1022;

			// The extrusion amount
			uniform float _ExtrusionLength = 0;

			//The first point
			uniform float4 _connectedSegmentFirstPoint;
			//The last point
			uniform float4 _connectedSegmentLastPoint;
			// From monotonically-increasing in u-parameter extruded points, packed as pairs into float4
            uniform float4 _increasingChunkConnectedSegmentPoints[_maxNumMonotonicChunkPoints];
			// From monotonically-increasing in u-parameter extruded point uvs, packed as pairs into float4
			uniform float4 _increasingChunkConnectedSegmentUvs[_maxNumMonotonicChunkPoints];
			// From monotonically-decreasing in u-parameter extruded points, packed as pairs into float4
			// Note that these points are ordered in increasing u-parameter, and matching u-parameters of the increasing points
            uniform float4 _decreasingChunkConnectedSegmentPoints[_maxNumMonotonicChunkPoints];
			// From monotonically-decreasing in u-parameter extruded point uvs, packed as pairs into float4
			// Note that these points are ordered in increasing u-parameter, and matching u-parameters of the increasing points
			uniform float4 _decreasingChunkConnectedSegmentUvs[_maxNumMonotonicChunkPoints];
			//Actual number of points in monotonically-increasing and monotonically-decreasing arrays
			uniform int _numberOfConnectedSegments;
			
			// vertex data transformation
			Interpolators vert (appdata v)
			{
				Interpolators o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.localxy = v.vertex.xy;
				o.segmentIndices = v.segmentIndices;
				return o;
			}

			// Use inverse bilinear interpolation to lookup point uv based on connected segment index defining which connected segment quad this fragment is in.
			float4 frag (Interpolators i) : SV_Target
			{
				float2 uvFinal = i.uv;

				//Rely on interpolation of i.segmentIndices to produce a result between connectedSegmentIndex and  connectedSegmentIndex + 1, due to our triangulation scheme
				int floorIndex = floor(i.segmentIndices.x);

				if(floorIndex < 0)
				{
					//keep regular uv of the final end triangulation
				}
				else if(floorIndex >= _numberOfConnectedSegments - 1)
				{
					//keep regular uv of the final end triangulation
				}
				else
				{
					uint connectedSegmentIndex = floorIndex;
					//First, unpack the points and uvs of the two points at connectedSegmentIndex
					uint startSegmentPackedIndex = connectedSegmentIndex / 2;
					bool startSegmentFirstHalfPacked = connectedSegmentIndex % 2 == 0;

					float4 increasingSegmentPointPacked = _increasingChunkConnectedSegmentPoints[startSegmentPackedIndex];
					float2 increasingSegmentPoint = startSegmentFirstHalfPacked ? increasingSegmentPointPacked.xy : increasingSegmentPointPacked.zw;
					float4 increasingSegmentUvPacked = _increasingChunkConnectedSegmentUvs[startSegmentPackedIndex];
					float2 increasingSegmentUv = startSegmentFirstHalfPacked ? increasingSegmentUvPacked.xy : increasingSegmentUvPacked.zw;

					float4 decreasingSegmentPointPacked = _decreasingChunkConnectedSegmentPoints[startSegmentPackedIndex];
					float2 decreasingSegmentPoint = startSegmentFirstHalfPacked ? decreasingSegmentPointPacked.xy : decreasingSegmentPointPacked.zw;
					float4 decreasingSegmentUvPacked = _decreasingChunkConnectedSegmentUvs[startSegmentPackedIndex];
					float2 decreasingSegmentUv = startSegmentFirstHalfPacked ? decreasingSegmentUvPacked.xy : decreasingSegmentUvPacked.zw;

					//Next, unpack the points and uvs of the two points at connectedSegmentIndex + 1
					uint endSegmentPackedIndex = (connectedSegmentIndex + 1) / 2;
					bool endSegmentFirstHalfPacked = (connectedSegmentIndex + 1) % 2 == 0;

					float4 nextIncreasingSegmentPointPacked = _increasingChunkConnectedSegmentPoints[endSegmentPackedIndex];
					float2 nextIncreasingSegmentPoint = endSegmentFirstHalfPacked ? nextIncreasingSegmentPointPacked.xy : nextIncreasingSegmentPointPacked.zw;
					float4 nextIncreasingSegmentUvPacked = _increasingChunkConnectedSegmentUvs[endSegmentPackedIndex];
					float2 nextIncreasingSegmentUv = endSegmentFirstHalfPacked ? nextIncreasingSegmentUvPacked.xy : nextIncreasingSegmentUvPacked.zw;

					float4 nextDecreasingSegmentPointPacked = _decreasingChunkConnectedSegmentPoints[endSegmentPackedIndex];
					float2 nextDecreasingSegmentPoint = endSegmentFirstHalfPacked ? nextDecreasingSegmentPointPacked.xy : nextDecreasingSegmentPointPacked.zw;
					float4 nextDecreasingSegmentUvPacked = _decreasingChunkConnectedSegmentUvs[endSegmentPackedIndex];
					float2 nextDecreasingSegmentUv = endSegmentFirstHalfPacked ? nextDecreasingSegmentUvPacked.xy : nextDecreasingSegmentUvPacked.zw;

					float4 increasingSegmentPointAndUv = float4(increasingSegmentPoint, increasingSegmentUv);
					float4 decreasingSegmentPointAndUv = float4(decreasingSegmentPoint, decreasingSegmentUv);
					float4 nextIncreasingSegmentPointAndUv = float4(nextIncreasingSegmentPoint, nextIncreasingSegmentUv);
					float4 nextDecreasingSegmentPointAndUv = float4(nextDecreasingSegmentPoint, nextDecreasingSegmentUv);

					uvFinal = InverseBilinearUv(i.localxy, increasingSegmentPointAndUv, decreasingSegmentPointAndUv, nextIncreasingSegmentPointAndUv, nextDecreasingSegmentPointAndUv);
				}

				float4 col = tex2D(_MainTex, TRANSFORM_TEX(uvFinal, _MainTex));
				
				return col;
			}

			ENDCG
		}
	}
}
