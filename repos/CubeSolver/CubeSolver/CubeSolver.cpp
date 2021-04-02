// CubeSolver.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>

class Cube {
private:
    int*** cube;
    int size;

public:
    Cube(int size) {
        this->size = size;
        cube = new int** [size];

        for (int i = 0; i < size; i++) {
            cube[i] = new int* [size];
            for (int j = 0; j < size; j++) {
                cube[i][j] = new int[size];
                for (int k = 0; k < size; k++) {
                    cube[i][j][k] = 0;
                }
            }
        }
    }

    void SetPoint(Point* point, int value) {
        cube[point->GetX()][point->GetY()][point->GetZ()] = value;
    }

    int GetPoint(Point* point) {
        return cube[point->GetX()][point->GetY()][point->GetZ()];
    }

    bool PlacePiece(Piece* piece) {
        Point* startPoint;
        bool isPointPlaceable = false;
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                for (int k = 0; k < size; k++) {
                    if (cube[i][j][k] == 0) {
                        startPoint = new Point(i, j, k);
                        
                        for (int n = 1; n < piece->GetSize(); n++) {
                            Point* point = piece->GetPoint(n);
                            if (this->GetPoint(point) != 0) {
                                break;
                            }
                        }


                    }
                }
            }
        }
    }
};

class Point {
    int* point;

public:
    Point(int x, int y, int z) {
        point = new int[3];
        point[0] = x;
        point[1] = y;
        point[2] = z;
    }

    int GetX() {
        return point[0];
    }
    int GetY() {
        return point[1];
    }
    int GetZ() {
        return point[2];
    }
};

class Piece {
    Point** piece;
    int size;

public:
    Piece(Point** points, int numPoints) {
        piece = points;
        size = numPoints;
    }

    Point* GetPoint(int location) {
        return piece[location];
    }

    int GetSize() {
        return size;
    }
};


int main()
{
    std::cout << "Hello World!\n";

    Cube* c = new Cube(5);

    Point** points = new Point*[2];
    points[0] = new Point(0, 0, 0);
    points[1] = new Point(1, 0, 0);

    Piece* p = new Piece(points, 2, 2);



}




// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
