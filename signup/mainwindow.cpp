#include "mainwindow.h"
#include "ui_mainwindow.h"


MainWindow::MainWindow(QWidget *parent) :
    QMainWindow(parent),
    ui(new Ui::MainWindow)
{
    ui->setupUi(this);
    manager = new QNetworkAccessManager(this);
    QObject::connect(manager, SIGNAL(finished(QNetworkReply*)), this, SLOT(finishedSlot(QNetworkReply*)));
    QObject::connect(manager, SIGNAL(finished(QNetworkReply*)), this, SLOT(finishedSlot(QNetworkReply*)));
}

MainWindow::~MainWindow()
{
    delete ui;
}

void MainWindow::finishedSlot(QNetworkReply *reply)
{
     if (reply->error() == QNetworkReply::NoError)
     {
         QByteArray bytes = reply->readAll();
         qDebug()<<"NoError"<<bytes.size();
         QString string = QString::fromUtf8(bytes);

         ui->textEdit->setText(string.toUtf8());
     }
     else
     {
         qDebug()<<"handle errors here";
         QVariant statusCodeV = reply->attribute(QNetworkRequest::HttpStatusCodeAttribute);
         //statusCodeV是HTTP服务器的相应码，reply->error()是Qt定义的错误码，可以参考QT的文档
         qDebug( "found error ....code: %d %d\n", statusCodeV.toInt(), (int)reply->error());
         qDebug(qPrintable(reply->errorString()));
     }
     reply->deleteLater();
}

void MainWindow::post()
{
//POST
    QNetworkRequest *request = new QNetworkRequest();
    //request->setUrl(QUrl("https://www.kuaidi100.com/autonumber/autoComNum?text=200382770316"));
    //request->setUrl(QUrl("http://reg.renren.com/AjaxRegisterAuth.do"));
    QString url="http://i.yiche.com/ajax/authenservice/MobileCode.ashx";
    request->setUrl(QUrl(url));
    request->setHeader(QNetworkRequest::ContentTypeHeader,"application/x-www-form-urlencoded");
//    request->setRawHeader("Cache-Control","no-cache");
//    request->setRawHeader("Connection","Keep-Alive");
//    request->setRawHeader("Content-Encoding","gzip");
//    request->setRawHeader("Content-Type","text/html;charset=utf-8");
//    request->setRawHeader("P3P","CP=CAO PSA OUR");
//    request->setRawHeader("Pragma","no-cache");
//    request->setRawHeader("Server","nginx/1.2.6");
//    request->setRawHeader("Transfer-Encoding","chunked");

   QByteArray postData;
   postData.append("mobileCodeKey=register&smsType=0&mobile=15140084363&imgCodeGuid=ee737db0-f044-7abd-aba4-9e68eabf5d83&imageCode=21549&Gamut=true");
   QNetworkReply* reply;
   reply = manager->post(*request,postData);
}

void MainWindow::get()
{
//GET

//    QUrl url("https://kyfw.12306.cn/otn/resources/js/framework/station_name.js");
//    QUrl url("http://localhost:8888/login");
//    QUrl url("http://dict.baidu.com/s?wd=name");

    QNetworkRequest *request = new QNetworkRequest();
    QString test="http://i.yiche.com/authenservice/common/CheckCode.aspx?guid=e45d4d30-ba91-a655-faf6-c3a6e1fec53c";
    request->setUrl(QUrl(test));
    QNetworkReply* reply = manager->get(*request);
}

void MainWindow::on_pushButton_clicked()
{
    post();
}

void MainWindow::on_pushButton_2_clicked()
{
    get();
}
