using System;
using System.Text;

public class Program
{
    public static void Main()
    {
        try
        {
            Console.Write("Enter size matrix: ");
            string? input = Console.ReadLine();

            if (!int.TryParse(input, out int size) || size <= 0)
            {
                throw new MatrixSizeException("Size matrix must be a positive integer");
            }

            SquareMatrix matrixA = new SquareMatrix(size, true);
            SquareMatrix matrixB = new SquareMatrix(size, true);

            Console.WriteLine("\nMatrix A:");
            Console.WriteLine(matrixA);

            Console.WriteLine("Matrix B:");
            Console.WriteLine(matrixB);

            Console.WriteLine("A + B:");
            Console.WriteLine(matrixA + matrixB);

            Console.WriteLine("A * B:");
            Console.WriteLine(matrixA * matrixB);

            Console.WriteLine($"det(A) = {matrixA.Determinant():F4}");

            if (matrixA)
            {
                Console.WriteLine("Matrix A is invertible:");
                Console.WriteLine(matrixA.Inverse());
            }
            else
            {
                Console.WriteLine("Matrix A is not invertible");
            }

            Console.WriteLine($"A == B : {matrixA == matrixB}");
            Console.WriteLine($"A > B : {matrixA > matrixB}");
        }
        catch (MatrixException exception)
        {
            Console.WriteLine($"Error: {exception.Message}");
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Unexpected error: {exception.Message}");
        }
    }
}

public class MatrixException : Exception
{
    public MatrixException(string message) : base(message) { }
}

public class MatrixSizeException : MatrixException
{
    public MatrixSizeException(string message) : base(message) { }
}

public class MatrixOperationException : MatrixException
{
    public MatrixOperationException(string message) : base(message) { }
}

public class SquareMatrix : ICloneable, IComparable<SquareMatrix>
{
    private readonly double[,] elements;
    private readonly int dimension;
    private static readonly Random random = new Random();
    private const double ZeroTolerance = 1e-9;
    private const int HashPrime = 31;
    private const int RandomMinValue = -5;
    private const int RandomMaxValue = 6;

    public int Size => dimension;

    public SquareMatrix(int size, bool generateRandom)
    {
        if (size <= 0)
        {
            throw new MatrixSizeException("Size matrix must be greater than zero");
        }

        dimension = size;
        elements = new double[size, size];

        if (generateRandom)
        {
            for (int rowIndex = 0; rowIndex < dimension; ++rowIndex)
            {
                for (int columnIndex = 0; columnIndex < dimension; ++columnIndex)
                {
                    elements[rowIndex, columnIndex] = random.Next(RandomMinValue, RandomMaxValue);
                }
            }
        }
    }

    public SquareMatrix(double[,] source)
    {
        if (source is null)
        {
            throw new MatrixOperationException("Source matrix is null");
        }

        int rows = source.GetLength(0);
        int columns = source.GetLength(1);

        if (rows != columns)
        {
            throw new MatrixSizeException("Matrix must be square");
        }

        dimension = rows;
        elements = (double[,])source.Clone();
    }

    public double this[int rowIndex, int columnIndex]
    {
        get
        {
            ValidateIndex(rowIndex, columnIndex);
            return elements[rowIndex, columnIndex];
        }
        set
        {
            ValidateIndex(rowIndex, columnIndex);
            elements[rowIndex, columnIndex] = value;
        }
    }

    private void ValidateIndex(int rowIndex, int columnIndex)
    {
        if (rowIndex < 0 || rowIndex >= dimension ||
            columnIndex < 0 || columnIndex >= dimension)
        {
            throw new MatrixOperationException("Index goes beyond matrix bounds");
        }
    }

    public static SquareMatrix operator +(SquareMatrix first, SquareMatrix second)
    {
        if (first is null || second is null)
        {
            throw new MatrixOperationException("Matrix is null");
        }

        EnsureSameSize(first, second);

        SquareMatrix result = new SquareMatrix(first.dimension, false);

        for (int rowIndex = 0; rowIndex < first.dimension; ++rowIndex)
        {
            for (int columnIndex = 0; columnIndex < first.dimension; ++columnIndex)
            {
                result[rowIndex, columnIndex] =
                    first[rowIndex, columnIndex] + second[rowIndex, columnIndex];
            }
        }

        return result;
    }

    public static SquareMatrix operator *(SquareMatrix first, SquareMatrix second)
    {
        if (first is null || second is null)
        {
            throw new MatrixOperationException("Matrix is null");
        }

        EnsureSameSize(first, second);

        SquareMatrix result = new SquareMatrix(first.dimension, false);

        for (int rowIndex = 0; rowIndex < first.dimension; ++rowIndex)
        {
            for (int columnIndex = 0; columnIndex < first.dimension; ++columnIndex)
            {
                double sum = 0;

                for (int innerIndex = 0; innerIndex < first.dimension; ++innerIndex)
                {
                    sum += first[rowIndex, innerIndex] *
                           second[innerIndex, columnIndex];
                }

                result[rowIndex, columnIndex] = sum;
            }
        }

        return result;
    }

    public static bool operator >(SquareMatrix first, SquareMatrix second)
    {
        return first.CompareTo(second) > 0;
    }

    public static bool operator <(SquareMatrix first, SquareMatrix second)
    {
        return first.CompareTo(second) < 0;
    }

    public static bool operator >=(SquareMatrix first, SquareMatrix second)
    {
        return first.CompareTo(second) >= 0;
    }

    public static bool operator <=(SquareMatrix first, SquareMatrix second)
    {
        return first.CompareTo(second) <= 0;
    }

    public static bool operator ==(SquareMatrix? first, SquareMatrix? second)
    {
        if (ReferenceEquals(first, second))
            return true;

        if (first is null || second is null)
            return false;

        if (first.dimension != second.dimension)
            return false;

        for (int rowIndex = 0; rowIndex < first.dimension; ++rowIndex)
        {
            for (int columnIndex = 0; columnIndex < first.dimension; ++columnIndex)
            {
                if (Math.Abs(first[rowIndex, columnIndex] -
                             second[rowIndex, columnIndex]) > ZeroTolerance)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public static bool operator !=(SquareMatrix? first, SquareMatrix? second)
    {
        return !(first == second);
    }

    public static bool operator true(SquareMatrix matrix)
    {
        return Math.Abs(matrix.Determinant()) > ZeroTolerance;
    }

    public static bool operator false(SquareMatrix matrix)
    {
        return Math.Abs(matrix.Determinant()) <= ZeroTolerance;
    }

    public static explicit operator double(SquareMatrix matrix)
    {
        return matrix.Determinant();
    }

    private static void EnsureSameSize(SquareMatrix first, SquareMatrix second)
    {
        if (first.dimension != second.dimension)
        {
            throw new MatrixOperationException("Matrices must be same size");
        }
    }

    public double Determinant()
    {
        if (dimension == 1)
            return elements[0, 0];

        if (dimension == 2)
            return elements[0, 0] * elements[1, 1] -
                   elements[0, 1] * elements[1, 0];

        double determinant = 0;

        for (int columnIndex = 0; columnIndex < dimension; ++columnIndex)
        {
            SquareMatrix minor = CreateMinor(0, columnIndex);
            double sign = columnIndex % 2 == 0 ? 1 : -1;
            determinant += sign * elements[0, columnIndex] * minor.Determinant();
        }

        return determinant;
    }

    private SquareMatrix CreateMinor(int excludedRow, int excludedColumn)
    {
        SquareMatrix minor = new SquareMatrix(dimension - 1, false);
        int minorRow = 0;

        for (int rowIndex = 0; rowIndex < dimension; ++rowIndex)
        {
            if (rowIndex == excludedRow)
                continue;

            int minorColumn = 0;

            for (int columnIndex = 0; columnIndex < dimension; ++columnIndex)
            {
                if (columnIndex == excludedColumn)
                    continue;

                minor[minorRow, minorColumn] = elements[rowIndex, columnIndex];
                ++minorColumn;
            }

            ++minorRow;
        }

        return minor;
    }

    public SquareMatrix Inverse()
    {
        double determinant = Determinant();

        if (Math.Abs(determinant) <= ZeroTolerance)
        {
            throw new MatrixOperationException("Matrix is degenerate");
        }

        SquareMatrix adjugate = new SquareMatrix(dimension, false);

        for (int rowIndex = 0; rowIndex < dimension; ++rowIndex)
        {
            for (int columnIndex = 0; columnIndex < dimension; ++columnIndex)
            {
                SquareMatrix minor = CreateMinor(rowIndex, columnIndex);
                double sign = (rowIndex + columnIndex) % 2 == 0 ? 1 : -1;

                adjugate[columnIndex, rowIndex] =
                    sign * minor.Determinant() / determinant;
            }
        }

        return adjugate;
    }

    public object Clone()
    {
        return new SquareMatrix((double[,])elements.Clone());
    }

    public int CompareTo(SquareMatrix? other)
    {
        if (other is null)
            return 1;

        return Determinant().CompareTo(other.Determinant());
    }

    public override bool Equals(object? obj)
    {
        return obj is SquareMatrix matrix && this == matrix;
    }

    public override int GetHashCode()
    {
        int hash = dimension;

        for (int rowIndex = 0; rowIndex < dimension; ++rowIndex)
        {
            for (int columnIndex = 0; columnIndex < dimension; ++columnIndex)
            {
                double value = Math.Abs(elements[rowIndex, columnIndex]) < ZeroTolerance
                    ? 0
                    : elements[rowIndex, columnIndex];

                hash = hash * HashPrime + value.GetHashCode();
            }
        }

        return hash;
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();

        for (int rowIndex = 0; rowIndex < dimension; ++rowIndex)
        {
            for (int columnIndex = 0; columnIndex < dimension; ++columnIndex)
            {
                double value = Math.Abs(elements[rowIndex, columnIndex]) < ZeroTolerance
                    ? 0
                    : elements[rowIndex, columnIndex];

                builder.Append($"{value, 8:F2}");
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }
}