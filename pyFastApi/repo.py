from peewee import *
from models import RezOfWork
from datetime import datetime
import json

cfg = json.load(open("config.json", "r"))

class BaseModel(Model):
    class Meta:
        database = PostgresqlDatabase(cfg["name"], host=cfg["host"], user=cfg["user"], password=cfg["password"])

class WorkerTable(BaseModel):
    id=AutoField(column_name='id')
    uniq=DecimalField(column_name='uniq')
    name=TextField(column_name='name')
    price=SmallIntegerField(column_name='price')
    workerstatus=BooleanField(column_name='workerstatus')

    class Meta:
        table_name = 'workerinfo'

class RezOfWorkTable(BaseModel):
    id=AutoField(column_name='id')
    uniq=DecimalField(column_name='uniq')
    name=TextField(column_name='name')
    project=TextField(column_name='project')
    tBegin=DateTimeField(column_name='tbegin')
    tEnd=DateTimeField(column_name='tend')
    timeOfWork=DecimalField(column_name='timeofwork')
    price=DecimalField(column_name='price')
    salary=DoubleField(column_name='salary')

    class Meta:
        table_name = 'rezofwork'

    def AddInfo(self):
        self.tEnd=datetime.now()
        self.timeOfWork=(self.tEnd-self.tBegin).total_seconds()/3600
        self.price=222
        self.salary=self.timeOfWork*self.price


        


def CheckStatus(uniq)->bool:
    print("Check",uniq)
    rez=WorkerTable.get(WorkerTable.uniq==uniq)
    print("h")
    return rez.workerstatus

def InsertNewWork(newwork:RezOfWork):
    print("Inserting",newwork.uniq,newwork.name,newwork.project,newwork.tBegin)
    RezOfWorkTable.create(uniq=newwork.uniq,name=newwork.name,project=newwork.project,tBegin=newwork.tBegin)
    query=WorkerTable.update(workerstatus=True).where(WorkerTable.uniq==5654825597)
    query.execute()

def InsertEndWork(uniq):
    rez=RezOfWorkTable.select().where(RezOfWorkTable.uniq==uniq).order_by(RezOfWorkTable.id.desc()).get()
    rez.AddInfo()
    print("->",rez.id,rez.uniq,rez.name,rez.project,rez.tBegin,rez.tEnd,rez.timeOfWork,rez.price,rez.salary)
    update=RezOfWorkTable.update(tEnd=rez.tEnd,timeOfWork=rez.timeOfWork,price=rez.price,salary=rez.salary).where(RezOfWorkTable.id==rez.id)
    update.execute()
    query=WorkerTable.update(workerstatus=False).where(WorkerTable.uniq==uniq)
    query.execute()
    return rez.__data__
# def main():
#     print("kek")
#     rez=RezOfWorkTable.select()
#     print(rez)
#     r1=rez.dicts().execute()
#     print(r1)
#     for item in r1:
#             print('artist: ', item["id"])

if __name__ == '__main__':
    rez=RezOfWorkTable.select().where(RezOfWorkTable.uniq==5654825597).order_by(RezOfWorkTable.id.desc()).get()
    rez.AddInfo()
    # print("->",rez.id,rez.uniq,rez.name,rez.project,rez.tBegin,rez.tEnd,rez.timeOfWork,rez.price,rez.salary)
    # update=RezOfWorkTable.update(tEnd=rez.tEnd,timeOfWork=rez.timeOfWork,price=rez.price,salary=rez.salary).where(RezOfWorkTable.id==rez.id)
    # update.execute()
    # query=WorkerTable.update(workerstatus=False).where(WorkerTable.uniq==5654825597)
    # query.execute()
    # worker=RezOfWork(rez.uniq,rez.name,rez.project,rez.tBegin)
    # print("worker",worker.uniq,worker.name,worker.project,worker.tBegin)
    # worker.AddInfo()
    # print("worker",worker.uniq,worker.name,worker.project,worker.tBegin,worker.tEnd,worker.timeOfWork,worker.price,worker.salary)
    #r1=rez.dicts().execute()
    # print(rez)
    # for item in rez:
    #         print('artist: ', item)
    print(rez.uniq,type(rez.uniq))
    print(type(rez))
    print(rez.__data__)




