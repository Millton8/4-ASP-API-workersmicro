from datetime import datetime


class RezOfWork():
    uniq:int
    name:str
    project:str
    tBegin:datetime
    tEnd:datetime
    timeOfWork:float
    price:int
    salary:float

    def __init__(self, uniq,name,project,tBegin=datetime.now()):
        self.uniq=uniq
        self.name=name
        self.project=project
        self.tBegin=tBegin
    def AddInfo(self):
        self.tEnd=datetime.now()
        self.timeOfWork=self.tEnd-self.tBegin
        self.price=222
        self.salary=self.timeOfWork.total_seconds()/3600*self.price