## Replacing Log Verify false
- Code before migration:
        this.Log.Verify(result.Found == false, "result should not have been found.");

- Code after migration:
        Assert.IsFalse(result.Found, "result should not have been found.");

## Replacing Log Verify
- Code before migration:
        this.Log.Verify

- Code after migration:
        Assert.IsTrue

## Replacing Log VerifyTrue
- Code before migration:
        this.Log.VerifyTrue

-Code after migration:
        Assert.IsTrue

## Replacing Log VerifyValue comparison
The Assert.AreEqual method requires the expected and actual values to be provided in the same order as specified by its method signature. 
The expected value should come first, followed by the actual value.
The order of these parameters matters.
Any hardcoded string, number must be considered as expected value and derived value from the implementation should be considered as actual value.

- Code before migration:
        int doWorkCount = 0;
        TestHook.RegisterHook("AtsJournalWorker::DoWorkAsync", _ => doWorkCount++);
        Log.VerifyValue(doWorkCount, 1, "wrong number of DoWork invocations (1) {0}", doWorkCount);

        public async Async.Task ReadAsPairsDouble()
        {
            List<AtsJournalSerializationBase> records = new List<AtsJournalSerializationBase>
            {
                GetRunRecord("a", 1),
                GetBaselineRecord("a", 1),
                GetRunRecord("b", 3),
                GetBaselineRecord("b", 3),
            };

            List<Tuple<AtsJournalSerializationBase, AtsJournalSerializationBase>> results = await CallReadAsPairs(records).WithCorrelation();
            Log.VerifyValue(results.Count, 2, "should be two");
            Log.VerifyValue(results[0].Item1.ItemId, "a", "should be for the expected document");
            Log.VerifyValue("b", results[1].Item1.ItemId, "should be for the expected document");
        }        

- Code after migration:
        int doWorkCount = 0;
        TestHook.RegisterHook("AtsJournalWorker::DoWorkAsync", _ => doWorkCount++);
        Assert.AreEqual(1, doWorkCount, "wrong number of DoWork invocations {0}", doWorkCount);
        
        public async Async.Task ReadAsPairsDouble()
        {
            List<AtsJournalSerializationBase> records = new List<AtsJournalSerializationBase>
            {
                GetRunRecord("a", 1),
                GetBaselineRecord("a", 1),
                GetRunRecord("b", 3),
                GetBaselineRecord("b", 3),
            };

            List<Tuple<AtsJournalSerializationBase, AtsJournalSerializationBase>> results = await CallReadAsPairs(records).WithCorrelation();
            Assert.AreEqual(2, results.Count, "should be two");
            Assert.AreEqual("a", results[0].Item1.ItemId, "should be for the expected document");
            Assert.AreEqual("b", results[1].Item1.ItemId, "should be for the expected document");
        }

## Replacing Log VerifyValue for boolean
If the actual value is a boolean type we can replace with Assert.IsTrue for 'True' value or Assert.IsFalse for 'False' expected value
- Code before migration:
        Log.VerifyValue(true, hitLogRunRecordWithoutBaselineTestHook, "should have noticed the missing record");

- Code after migration:
        Assert.IsTrue(hitLogRunRecordWithoutBaselineTestHook, "should have noticed the missing record");

## Replacing Log Verify Not Null
- Code before migration:
        this.Log.VerifyNotNull

- Code after migration:
        Assert.IsNotNull

## Replacing Log Verify False
- Code before migration:
        this.Log.VerifyFalse

- Code after migration:
        Assert.IsFalse

## Replacing Log Verify Null
- Code before migration:
        this.Log.VerifyNull

- Code after migration:
        Assert.IsNull
		
## Replacing Log Fail
- Code before migration:
        this.Log.Fail("should have failed");

- Code after migration:
        Assert.Fail("should have failed");
		
## Replacing Log Pass
- Code before migration:
        this.Log.Pass("pass");

- Code after migration:
        Debug.WriteLine("pass");
		
## Replacing Log Assert
- Code before migration:
        this.Log.Assert

- Code after migration:
        Assert.IsTrue