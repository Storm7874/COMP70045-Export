#Encoder test data generator
#Supplied as part of the MSc Dissertation Project - LoRa Emergency Messaging System
# Matthew Boyce - b018567j@student.staffs.ac.uk

import csv, json, os, random

## Open dictionary/wordlist
wordList = []

## Define the max message sizes to generate
MaxMessageSize = 200
MinMessageSize = 2

InputFile = 'unigram_freq.csv'

## Open wordlist and populate word array
with open(InputFile, 'r') as csvfile:
    reader = csv.reader(csvfile)
    for row in reader:
        if row:
            wordList.append(row[0])

## Classes to hold encoder functionality
class Encoder():
    def __init__(self):
        pass

    ##Dictionary encoding scheme model
    def encode(self, wordList):
        wordCount = 0
        for word in wordList:
            #As each wordRef object is 2.5 bytes in size, regardless of the word, the returned object size will
            #always be 2.5 bytes.

            wordCount += 2.5

        return wordCount


class RawEncoder():
    def __init__(self):
        pass

    ##Sequential encoding scheme model
    def encode(self, wordList):
        charCount = 0
        for word in wordList:
            ##Get the character count from each word
            charCount += len(word)

        ##Each character is two hex digits, multiply the character count by the total number of hex digits
        MessageTotalCount = charCount * 2

        return(MessageTotalCount)

DictEncoder = Encoder()
RawEncoder = RawEncoder()

##Generate a random message, up to the max words parameter
def getRandomWords(Max):
    messageWordList = []
    for count in range(0,Max):
        messageWordList.append(wordList[random.randint(0,(len(wordList) - 1))])

    return messageWordList

RawSizeList = []
EncodedSizeList = []

##Method for one test iteration, up to maxMessageLength words
def iterateTest(maxMessageLength):
    for count in range(0, iterations):
        TestMessage = getRandomWords(maxMessageLength)
        rawEncodeSize = RawEncoder.encode(TestMessage)
        encodedSize = DictEncoder.encode(TestMessage)

        ##Append encoded size for this iteration to the respective output array
        RawSizeList.append(rawEncodeSize)
        EncodedSizeList.append(encodedSize)

    #After getting both encode sizes for 'iterations' number of times, display output:
    print("MAX MESSAGE LENGTH: {}".format(maxMessageLength))
    print("""
    RAW LIST                                DICT LIST
    Success:    {}                          {}
    Fail:       {}                          {}
    Min:        {}                          {}
    Max:        {}                          {}
    Avg:        {}                          {}
    Compression Factor: {}
    """.format(len(RawSizeList),len(EncodedSizeList),
               (iterations - len(RawSizeList)),(iterations - len(EncodedSizeList)),
               min(RawSizeList), min(EncodedSizeList),
               max(RawSizeList), max(EncodedSizeList),
               (sum(RawSizeList) / len(RawSizeList)),  (sum(EncodedSizeList) / len(EncodedSizeList)),
               ((sum(EncodedSizeList) / len(EncodedSizeList)) / (sum(RawSizeList) / len(RawSizeList)) * 100)
               ))

    RawSizeList.clear()
    EncodedSizeList.clear()

##Number of iterations to run each test
iterations = 10000000

##Message size bins (Words)
messageLengthList = [2,3,4,5,10,20,50,100,200]

##Perform iterations number of tests on each message length category
for mml in messageLengthList:
    iterateTest(mml)













