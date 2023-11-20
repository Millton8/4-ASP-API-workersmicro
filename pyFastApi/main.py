from fastapi import FastAPI, Body
from fastapi.responses import HTMLResponse, FileResponse, Response,JSONResponse
from repo import CheckStatus,InsertNewWork,InsertEndWork
from models import RezOfWork



app = FastAPI()
 

@app.post("/newwork")
def root(response: Response,data = Body()):
    st=CheckStatus(data["ID"])
    if st==True:
        response.status_code=201
        return {data['ID']:"Уже работает"}
    worker= RezOfWork(data["ID"],data["name"],data["project"])
    InsertNewWork(worker)


@app.patch("/endwork/{uniq}")
def root(uniq,response: Response):
    print("Endwork with")
    st=CheckStatus(uniq)
    if st==False:
        response.status_code=201
        print('Не работает',uniq)
        return {uniq:"Не работает"}
    rez=InsertEndWork(uniq)

    return rez