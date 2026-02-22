using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberTools
{
    public static string GetNumberConversion(long value)
    {
        if (value < 10000) return value.ToString();  //如果要算负值，则取绝对值计算
        
        long number = value;
        int index = -1;
        while (number > 0)
        {
            index++;
            number = number / 1000;
        }
        //index = 0 千以内 不换算  =1代表千  划算数值 
        string endValue = null;

        switch (index)
        {
            case 0:
                return value.ToString();
            default:
                endValue = Conversion(value, index);
                endValue += GetConversionEnd(index);
                break;

        }

        return endValue.ToString();
    }

    protected static string Conversion(long value, int index)
    {
        long endValue = value;

        for (int i = 0; i < index; i++)
        {
            endValue = endValue / 1000;

        }

        string str = endValue.ToString();
        if (str.Length == 1)
        {

            str = str + "." + value.ToString()[1];
        }

        return str;
    }

    protected static string GetConversionEnd(int index)
    {

        switch (index)
        {
            case 1:
                return "K";
            case 2:
                return "M";
            case 3:
                return "T";
            case 4:
                return "P";
            case 5:
                return "E";
            case 6:
                return "Z";
        }

        return "";
    }
}
