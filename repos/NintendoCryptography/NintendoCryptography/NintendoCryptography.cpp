
#include <iostream>
#include <iomanip>

using namespace std;

int main()
{
    int size;

    cin >> size;

    unsigned int* a = new unsigned int[size / 16]; // <- input tab to encrypt
    unsigned int* b = new unsigned int[size / 16]; // <- output tab

    for (int i = 0; i < size / 16; i++) {   // Read size / 16 integers to a
        cin >> hex >> a[i];
    }

    for (int i = 0; i < size / 16; i++) {   // Write size / 16 zeros to b
        b[i] = 0;
    }

    for (int i = 0; i < size; i++) {
        for (int j = 0; j < size; j++) {
            int bIndex = (i + j) / 32;
            int aIndex = j / 32 + size / 32;
            int leftAnd = (a[i / 32] >> (i % 32));
            int rightAnd = (a[aIndex] >> (j % 32)) & 1;
            int leftShift = ((i + j) % 32);
            int powerRaise = (leftAnd & rightAnd) << leftShift;
            b[bIndex] ^= powerRaise;  // Magic centaurian operation
        }
    }

    for (int i = 0; i < size / 16; i++) {
        if (i > 0) {
            cout << ' ';
        }
        cout << setfill('0') << setw(8) << hex << b[i];       // print result
    }
    cout << endl;

    /*
       Good luck humans
    */
    return 0;
}