﻿using System;
using System.Collections.Generic;

namespace AgOpenGPS
{
    public class CGeoFence
    {
        //copy of the mainform address
        private readonly FormGPS mf;

        /// <summary>
        /// array of turns
        /// </summary>
        public List<CGeoFenceLines> geoFenceArr = new List<CGeoFenceLines>();

        //constructor
        public CGeoFence(FormGPS _f)
        {
            mf = _f;
        }


        public void FindPointsDriveAround(vec3 fromPt, double headAB, ref vec3 start, ref vec3 stop)
        {
            //initial scan is straight ahead of pivot point of vehicle to find the right turnLine/boundary
            vec3 pt = new vec3();

            bool isFound = false;
            int closestTurnNum = 99;
            int closestTurnIndex = 99999;
            int mazeDim = mf.mazeGrid.mazeColXDim * mf.mazeGrid.mazeRowYDim;

            double cosHead = Math.Cos(headAB);
            double sinHead = Math.Sin(headAB);

            for (int b = 1; b < 1600; b += 2)
            {
                pt.easting = fromPt.easting + (sinHead * b);
                pt.northing = fromPt.northing + (cosHead * b);

                if (mf.turn.turnArr.Count > mf.bnd.LastBoundary && mf.bnd.LastBoundary >= 0 && mf.turn.turnArr[mf.bnd.LastBoundary].IsPointInTurnWorkArea(pt))
                {
                    for (int t = 0; t < mf.bnd.bndArr.Count; t++)
                    {
                        if (mf.bnd.bndArr[t].isDriveThru || mf.bnd.bndArr[t].isOwnField) continue;
                        //skip unnecessary boundaries
                        if (mf.bnd.bndArr[t].OuterField == mf.bnd.LastBoundary || mf.bnd.bndArr[t].OuterField == -1)
                        {

                            if (mf.bnd.bndArr[t].isDriveAround)
                            {
                                if (mf.gf.geoFenceArr[t].IsPointInGeoFenceArea(pt))
                                {
                                    isFound = true;
                                    closestTurnNum = t;
                                    closestTurnIndex = b;

                                    start.easting = fromPt.easting + (sinHead * b);
                                    start.northing = fromPt.northing + (cosHead * b);
                                    start.heading = headAB;
                                    break;
                                }
                            }
                            else
                            {
                                //its a uturn obstacle
                                if (mf.turn.turnArr[t].IsPointInTurnWorkArea(pt))
                                {
                                    start.easting = 88888;
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    start.easting = 88888;
                    return;
                }
                if (isFound) break;
            }

            isFound = false;

            for (int b = closestTurnIndex + 200; b > closestTurnIndex; b--)
            {
                pt.easting = fromPt.easting + (sinHead * b);
                pt.northing = fromPt.northing + (cosHead * b);

                if (mf.gf.geoFenceArr[closestTurnNum].IsPointInGeoFenceArea(pt))
                {
                    isFound = true;

                    stop.easting = fromPt.easting + (sinHead * b);
                    stop.northing = fromPt.northing + (cosHead * b);
                    stop.heading = headAB;
                }

                if (isFound) break;
            }

            for (int i = 0; i < 30; i++)
            {
                start.easting -= sinHead;
                start.northing -= cosHead;
                start.heading = headAB;

                int iStart = (int)((((int)((start.northing - mf.minFieldY) / mf.mazeGrid.mazeScale)) * mf.mazeGrid.mazeColXDim)
                    + (int)((start.easting - mf.minFieldX) / mf.mazeGrid.mazeScale));

                if (iStart >= mazeDim)
                {
                    start.easting = 88888;
                    return;
                }

                if (mf.mazeGrid.mazeArr[iStart] == 0) break;
            }

            for (int i = 0; i < 30; i++)
            {
                stop.easting += sinHead;
                stop.northing += cosHead;
                stop.heading = headAB;

                int iStop = (int)((((int)((stop.northing - mf.minFieldY) / mf.mazeGrid.mazeScale)) * mf.mazeGrid.mazeColXDim)
                    + (int)((stop.easting - mf.minFieldX) / mf.mazeGrid.mazeScale));

                if (iStop >= mazeDim)
                {
                    start.easting = 88888;
                    return;
                }

                if (mf.mazeGrid.mazeArr[iStop] == 0) break;
            }
        }

        public bool IsPointInsideGeoFences(vec3 pt)
        {


            bool test = false;

            for (int b = 0; b < mf.bnd.bndArr.Count; b++)
            {
                if (mf.bnd.bndArr[b].isOwnField)
                {
                    if (geoFenceArr[b].IsPointInGeoFenceArea(pt))
                    {
                        test = true;
                        for (int c = 0; c < mf.bnd.bndArr.Count; c++)
                        {
                            //skip unnecessary boundaries
                            if (!mf.bnd.bndArr[c].isOwnField && (mf.bnd.bndArr[c].OuterField == b || mf.bnd.bndArr[c].OuterField == -1))
                            {
                                if (geoFenceArr[c].IsPointInGeoFenceArea(pt))
                                {
                                    //point is in an inner area but inside outer
                                    test = false;
                                }
                            }
                        }
                        break;//overlap doesnt matter if both outerbounds?
                    }
                }
            }
            return test;
        }

        public bool IsPointInsideGeoFences(vec2 pt)
        {
            bool test = false;
            for (int b = 0; b < mf.bnd.bndArr.Count; b++)
            {
                if (mf.bnd.bndArr[b].isOwnField)
                {
                    if (geoFenceArr[b].IsPointInGeoFenceArea(pt))
                    {
                        test = true;
                        for (int c = 0; c < mf.bnd.bndArr.Count; c++)
                        {
                            //skip unnecessary boundaries
                            if (!mf.bnd.bndArr[c].isOwnField && (mf.bnd.bndArr[c].OuterField == b || mf.bnd.bndArr[c].OuterField == -1))
                            {
                                if (geoFenceArr[c].IsPointInGeoFenceArea(pt))
                                {
                                    //point is in an inner area but inside outer
                                    test = false;
                                }
                            }
                        }
                        break;//overlap doesnt matter if both outerbounds?
                    }
                }
            }
            return test;
        }

        public void BuildGeoFenceLines(int Num)
        {
            if (mf.bnd.bndArr.Count == 0)
            {
                //mf.TimedMessageBox(1500, " No Boundary ", "No GeoFence Made");
                return;
            }

            //to fill the list of line points
            vec3 point = new vec3();

            //inside boundaries
            for (int j = (Num < 0) ? 0 : Num; j < mf.bnd.bndArr.Count; j++)
            {
                geoFenceArr[j].geoFenceLine.Clear();
                //if (!mf.bnd.bndArr[j].isOwnField && mf.bnd.bndArr[j].isDriveThru) continue;

                int ChangeDirection = mf.bnd.bndArr[j].isOwnField ? -1 : 1;

                int ptCount = mf.bnd.bndArr[j].bndLine.Count;

                for (int i = ptCount - 1; i >= 0; i--)
                {
                    //calculate the point outside the boundary
                    point.easting = mf.bnd.bndArr[j].bndLine[i].easting + (-Math.Sin(glm.PIBy2 + mf.bnd.bndArr[j].bndLine[i].heading) * mf.yt.geoFenceDistance * ChangeDirection);
                    point.northing = mf.bnd.bndArr[j].bndLine[i].northing + (-Math.Cos(glm.PIBy2 + mf.bnd.bndArr[j].bndLine[i].heading) * mf.yt.geoFenceDistance * ChangeDirection);
                    point.heading = mf.bnd.bndArr[j].bndLine[i].heading;

                    //only add if outside actual field boundary
                    //if ((mf.bnd.bndArr[j].isOwnField && mf.bnd.bndArr[j].IsPointInsideBoundary(point)) || (!mf.bnd.bndArr[j].isOwnField && !mf.bnd.bndArr[j].IsPointInsideBoundary(point)))
                    {
                        vec2 tPnt = new vec2(point.easting, point.northing);
                        geoFenceArr[j].geoFenceLine.Add(tPnt);
                    }
                }
                geoFenceArr[j].FixGeoFenceLine(mf.yt.geoFenceDistance, mf.bnd.bndArr[j].bndLine, mf.tool.toolWidth * 0.5);
                geoFenceArr[j].PreCalcTurnLines();

                if (Num > -1) break;
            }
        }

        public void DrawGeoFenceLines()
        {
            for (int i = 0; i < mf.bnd.bndArr.Count; i++)
            {
                geoFenceArr[i].DrawGeoFenceLine();
            }
        }
    }
}