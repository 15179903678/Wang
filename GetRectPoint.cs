using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;

namespace iWhiteBoard
{
        public class StrokeInfo
        {
            public StrokeInfo(StylusPointCollection s, DrawingAttributes drawingAttributes)
            {
                this.stylus = s;
                this.draw = drawingAttributes;
            }
            public StrokeInfo()
            {
                this.stylus = new StylusPointCollection();
            }
            public int Index { get; set; }
            public StylusPointCollection stylus { get; set; }
            public DrawingAttributes draw { get; set; }
        }
        public class GetRectPoint
        {
            #region 计算矩形区域与线段的交点
            //pointfIn1        线段第一点ptIn1
            //pointfIn2        第二点坐标ptIn2
            //rect             剪裁区域所在矩形坐标rect
            //pointfOut1   如果有交点，保存靠近ptIn1的交点，否则该参数返回无效。
            //pointfOut2   如果有2个交点，保存靠近ptIn2的交点，否则该参数返回无效。
            //int          交点个数
            //无
            #endregion
            public static int GetIntersectPointFs(Rect rect, Point pointfIn1, Point pointfIn2, out Point pointfOut1, out Point pointfOut2)
            {
                pointfOut1 = new Point();
                pointfOut2 = new Point();
                Point[] pointfs = new Point[4];
                Rect rectLine;
                int nIntersectCount = 0;
                rectLine = new Rect(pointfIn1, pointfIn2);
                if (0 == rectLine.Width)
                {
                    rectLine.Width += 4;
                }
                if (0 == rectLine.Height)
                {
                    rectLine.Height += 4;
                }
                //计算线段与矩形区左边的交点
                pointfs[nIntersectCount].X = rect.Left;
                pointfs[nIntersectCount].Y = (pointfIn2.Y - pointfIn1.Y) / (pointfIn2.X - pointfIn1.X) * (pointfs[nIntersectCount].X - pointfIn1.X) + pointfIn1.Y;
                if (FptInRect(pointfs[nIntersectCount], rectLine) && FptInRect(pointfs[nIntersectCount], rect))
                {
                    //小量偏移，使之不在板擦区域内
                    pointfs[nIntersectCount].X -= 1.0f;
                    pointfs[nIntersectCount].Y = (pointfIn2.Y - pointfIn1.Y) / (pointfIn2.X - pointfIn1.X) * (pointfs[nIntersectCount].X - pointfIn1.X) + pointfIn1.Y;
                    nIntersectCount++;
                }
                //计算线段与矩形区右边的交点
                pointfs[nIntersectCount].X = rect.Right;
                pointfs[nIntersectCount].Y = (pointfIn2.Y - pointfIn1.Y) / (pointfIn2.X - pointfIn1.X) * (pointfs[nIntersectCount].X - pointfIn1.X) + pointfIn1.Y;
                if (FptInRect(pointfs[nIntersectCount], rectLine) && FptInRect(pointfs[nIntersectCount], rect))
                {
                    pointfs[nIntersectCount].X += 1.0f;
                    pointfs[nIntersectCount].Y = (pointfIn2.Y - pointfIn1.Y) / (pointfIn2.X - pointfIn1.X) * (pointfs[nIntersectCount].X - pointfIn1.X) + pointfIn1.Y;
                    nIntersectCount++;
                }
                //当找到两个交点时可不用再找交点，最多两个
                if (2 != nIntersectCount)
                {
                    //计算线段与矩形区上边的交点
                    pointfs[nIntersectCount].Y = rect.Top;
                    pointfs[nIntersectCount].X = (pointfIn2.X - pointfIn1.X) / (pointfIn2.Y - pointfIn1.Y) * (pointfs[nIntersectCount].Y - pointfIn1.Y) + pointfIn1.X;
                    if (FptInRect(pointfs[nIntersectCount], rectLine) && FptInRect(pointfs[nIntersectCount], rect))
                    {
                        pointfs[nIntersectCount].Y -= 1.0f;
                        pointfs[nIntersectCount].X = (pointfIn2.X - pointfIn1.X) / (pointfIn2.Y - pointfIn1.Y) * (pointfs[nIntersectCount].Y - pointfIn1.Y) + pointfIn1.X;
                        nIntersectCount++;
                    }
                    if (2 != nIntersectCount)
                    {
                        //计算线段与矩形区下边的交点
                        pointfs[nIntersectCount].Y = rect.Bottom;
                        pointfs[nIntersectCount].X = (pointfIn2.X - pointfIn1.X) / (pointfIn2.Y - pointfIn1.Y) * (pointfs[nIntersectCount].Y - pointfIn1.Y) + pointfIn1.X;
                        if (FptInRect(pointfs[nIntersectCount], rectLine) && FptInRect(pointfs[nIntersectCount], rect))
                        {
                            pointfs[nIntersectCount].Y += 1.0f;
                            pointfs[nIntersectCount].X = (pointfIn2.X - pointfIn1.X) / (pointfIn2.Y - pointfIn1.Y) * (pointfs[nIntersectCount].Y - pointfIn1.Y) + pointfIn1.X;
                            nIntersectCount++;
                        }
                    }
                }
                if (0 == nIntersectCount)
                {
                    pointfOut1 = new Point();
                    pointfOut2 = new Point();
                    return 0;
                }
                if (1 == nIntersectCount)
                {
                    pointfOut1.X = pointfs[0].X;
                    pointfOut1.Y = pointfs[0].Y;
                    pointfOut2 = new Point();
                    return 1;
                }

                //调整相交点的顺序，使用相交点的第1点靠近线段第1点，相交点的第2点靠近线段第2点
                if (pointfIn1.X == pointfIn2.X) //垂直线
                {
                    if (pointfIn1.Y > pointfIn2.Y)
                    {
                        if (pointfs[0].Y > pointfs[1].Y)
                        {
                            pointfOut1.X = pointfs[0].X;
                            pointfOut1.Y = pointfs[0].Y;
                            pointfOut2.X = pointfs[1].X;
                            pointfOut2.Y = pointfs[1].Y;
                        }
                        else
                        {
                            pointfOut1.X = pointfs[1].X;
                            pointfOut1.Y = pointfs[1].Y;
                            pointfOut2.X = pointfs[0].X;
                            pointfOut2.Y = pointfs[0].Y;
                        }
                    }
                    else
                    {
                        if (pointfs[0].Y < pointfs[1].Y)
                        {
                            pointfOut1.X = pointfs[0].X;
                            pointfOut1.Y = pointfs[0].Y;
                            pointfOut2.X = pointfs[1].X;
                            pointfOut2.Y = pointfs[1].Y;
                        }
                        else
                        {
                            pointfOut1.X = pointfs[1].X;
                            pointfOut1.Y = pointfs[1].Y;
                            pointfOut2.X = pointfs[0].X;
                            pointfOut2.Y = pointfs[0].Y;
                        }
                    }
                }
                else if (pointfIn1.X < pointfIn2.X)
                {
                    if (pointfs[0].X < pointfs[1].X)
                    {
                        pointfOut1.X = pointfs[0].X;
                        pointfOut1.Y = pointfs[0].Y;
                        pointfOut2.X = pointfs[1].X;
                        pointfOut2.Y = pointfs[1].Y;
                    }
                    else
                    {
                        pointfOut1.X = pointfs[1].X;
                        pointfOut1.Y = pointfs[1].Y;
                        pointfOut2.X = pointfs[0].X;
                        pointfOut2.Y = pointfs[0].Y;
                    }
                }
                else
                {
                    if (pointfs[0].X > pointfs[1].X)
                    {
                        pointfOut1.X = pointfs[0].X;
                        pointfOut1.Y = pointfs[0].Y;
                        pointfOut2.X = pointfs[1].X;
                        pointfOut2.Y = pointfs[1].Y;
                    }
                    else
                    {
                        pointfOut1.X = pointfs[1].X;
                        pointfOut1.Y = pointfs[1].Y;
                        pointfOut2.X = pointfs[0].X;
                        pointfOut2.Y = pointfs[0].Y;
                    }
                }
                return nIntersectCount;
            }
            public static bool FptInRect(Point pPtF, Rect lpRc)
            {
                if (((pPtF.X >= lpRc.Left) && (pPtF.X <= lpRc.Right)) &&
                    ((pPtF.Y >= lpRc.Top) && (pPtF.Y <= lpRc.Bottom)))
                {
                    return true;
                }
                return false;
            }
            public static bool eraserPoint(Rect eraserRectF, StrokeCollection Strokes, out List<StrokeInfo> NewAll)
            {
                List<List<Point>> list = new List<List<Point>>();
                List<DrawingAttributes> draws = new List<DrawingAttributes>();
                NewAll = new List<StrokeInfo>();
                foreach (var item in Strokes)
                {
                    List<Point> Stroke = new List<Point>();
                    foreach (var key in item.StylusPoints)
                    {
                        Stroke.Add(new Point(key.X, key.Y));
                    }
                    list.Add(Stroke);
                    draws.Add(item.DrawingAttributes);
                }
                if (list == null)
                {
                    return false;
                }
                if (draws.Count != list.Count)
                {
                    return false;
                }
                Point[] intersectionPointInfos = new Point[] { new Point(), new Point() };
                Point[] srcPointInfos = new Point[] { new Point(), new Point() };
                int nIntersectCount;
                for (int i = 0; i < list.Count; i++)
                {
                    List<Point> lintList = list[i];
                    int size = lintList.Count;
                    if (size == 0)
                    {
                        continue;
                    }
                    if (size == 1)
                    {
                        srcPointInfos[0] = lintList[0];
                        if (FptInRect(srcPointInfos[0], eraserRectF))
                        {
                            continue;
                        }
                        else
                        {
                            StrokeInfo s = new StrokeInfo();
                            foreach (var item in lintList)
                            {
                                s.stylus.Add(new StylusPoint(item.X, item.Y));
                            }
                            s.draw = draws[i];
                            s.Index = i;
                            NewAll.Add(s);
                            continue;
                        }
                    }
                    srcPointInfos[0] = lintList[0];
                    List<Point> tempList = new List<Point>();
                    Rect lineRect = new Rect();
                    if (!FptInRect(srcPointInfos[0], eraserRectF))
                    {
                        tempList.Add(lintList[0]);
                    }
                    for (int j = 1; j < size; j++)
                    {
                        srcPointInfos[1] = lintList[j];
                        double l = srcPointInfos[0].X;
                        double t = srcPointInfos[0].Y;
                        double r = srcPointInfos[1].X;
                        double b = srcPointInfos[1].Y;
                        if (l > r)
                        {
                            l = srcPointInfos[1].X;
                            r = srcPointInfos[0].X;
                        }
                        if (t > b)
                        {
                            t = srcPointInfos[1].Y;
                            b = srcPointInfos[0].Y;
                        }
                        lineRect = new Rect(new Point(l, t), new Point(r, b));
                        if (lineRect.Width == 0)
                            lineRect = new Rect(new Point(l, t), new Point(r + 4, b));
                        if (lineRect.Height == 0)
                            lineRect = new Rect(new Point(l, t), new Point(r, b + 4));
                        //判断两个矩形是否相交
                        if (!lineRect.IntersectsWith(eraserRectF))
                        {
                            tempList.Add(lintList[j]);
                            srcPointInfos[0].X = srcPointInfos[1].X;
                            srcPointInfos[0].Y = srcPointInfos[1].Y;
                            continue;
                        }
                        else
                        {
                            //判断线段处矩形的交点
                            nIntersectCount = GetIntersectPointFs(eraserRectF, srcPointInfos[0], srcPointInfos[1], out intersectionPointInfos[0], out intersectionPointInfos[1]);
                        }
                        //点1是否在矩形中
                        if (FptInRect(srcPointInfos[0], eraserRectF))
                        {
                            srcPointInfos[0].X = srcPointInfos[1].X;
                            srcPointInfos[0].Y = srcPointInfos[1].Y;
                            if (nIntersectCount == 0)
                            {
                                //点2在矩形中
                                continue;
                            }
                            else if (nIntersectCount > 0)
                            {
                                //点2不在矩形中
                                if (tempList.Count > 0)
                                {
                                    List<Point> copy = new List<Point>();
                                    copy.AddRange(tempList);
                                    StrokeInfo s = new StrokeInfo();
                                    foreach (var item in copy)
                                    {
                                        s.stylus.Add(new StylusPoint(item.X, item.Y));
                                    }
                                    s.draw = draws[i];
                                    s.Index = i;
                                    NewAll.Add(s);
                                    tempList.Clear();
                                }
                                Point currentP = lintList[j];
                                if ((intersectionPointInfos[0].X - currentP.X < -3.0f) || (intersectionPointInfos[0].X - currentP.X > 3.0f)
                                        || (intersectionPointInfos[0].Y - currentP.Y < -3.0f) || (intersectionPointInfos[0].Y - currentP.Y > 3.0f))
                                {
                                    Point StylusPoint = new Point(intersectionPointInfos[0].X, intersectionPointInfos[0].Y);
                                    tempList.Add(StylusPoint);
                                    tempList.Add(currentP);
                                }
                                else
                                {
                                    tempList.Add(currentP);
                                }
                                continue;
                            }
                            else
                            {

                            }
                        }
                        else
                        {
                            srcPointInfos[0].X = srcPointInfos[1].X;
                            srcPointInfos[0].Y = srcPointInfos[1].Y;
                            //点1在外，点2在内 
                            if (nIntersectCount == 1)
                            {
                                Point StylusPoint = new Point(intersectionPointInfos[0].X, intersectionPointInfos[0].Y);
                                tempList.Add(StylusPoint);
                                List<Point> copy = new List<Point>();
                                copy.AddRange(tempList);
                                StrokeInfo s = new StrokeInfo();
                                foreach (var item in copy)
                                {
                                    s.stylus.Add(new StylusPoint(item.X, item.Y));
                                }
                                s.draw = draws[i];
                                s.Index = i;
                                NewAll.Add(s);
                                tempList.Clear();
                                continue;
                            }
                            else if (nIntersectCount == 2)
                            {
                                //点1点2都在外
                                //点2不在矩形中
                                if (tempList.Count > 0)
                                {
                                    Point lastP =new Point( tempList[tempList.Count - 1].X, tempList[tempList.Count - 1].Y);
                                    if ((intersectionPointInfos[0].X - lastP.X < -3.0f) || (intersectionPointInfos[0].X - lastP.X > 3.0f)
                                            || (intersectionPointInfos[0].Y - lastP.Y < -3.0f) || (intersectionPointInfos[0].Y - lastP.Y > 3.0f))
                                    {
                                        Point addP = new Point(intersectionPointInfos[0].X, intersectionPointInfos[0].Y);
                                        tempList.Add(addP);
                                    }
                                }
                                else
                                {
                                    Point addP = new Point(intersectionPointInfos[0].X, intersectionPointInfos[0].Y);
                                    tempList.Add(addP);
                                }

                                List<Point> copy = new List<Point>();
                                copy.AddRange(tempList);
                                StrokeInfo s = new StrokeInfo();
                                foreach (var item in copy)
                                {
                                    s.stylus.Add(new StylusPoint(item.X, item.Y));
                                }
                                s.draw = draws[i];
                                s.Index = i;
                                NewAll.Add(s);
                                tempList.Clear();

                                Point currentP = lintList[j];
                                if ((intersectionPointInfos[1].X - currentP.X < -1.0f) || (intersectionPointInfos[1].X - currentP.X > 1.0f)
                                            || (intersectionPointInfos[1].Y - currentP.Y < -1.0f) || (intersectionPointInfos[1].Y - currentP.Y > 1.0f))
                                {
                                    Point StylusPoint1 = new Point(intersectionPointInfos[1].X, intersectionPointInfos[1].Y);
                                    tempList.Add(StylusPoint1);
                                    tempList.Add(currentP);
                                }
                                else
                                {
                                    tempList.Add(currentP);
                                }
                                continue;
                            }
                            else
                            {
                                tempList.Add(lintList[j]);
                                continue;
                            }
                        }
                    }
                    if (tempList.Count > 0)
                    {
                        List<Point> copy = new List<Point>();
                        copy.AddRange(tempList);
                        StrokeInfo s = new StrokeInfo();
                        foreach (var item in copy)
                        {
                            s.stylus.Add(new StylusPoint(item.X, item.Y));
                        }
                        s.draw = draws[i];
                        s.Index = i;
                        NewAll.Add(s);
                        tempList.Clear();
                    }
                }
                return true;
            }
        }
}
