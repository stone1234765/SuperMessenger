import React, {useState, useEffect, useRef} from 'react';
import Oidc from "oidc-client"
// import { render } from '@testing-library/react';
import Navbar from './Organisms/Navbar';
import MainPage from './Organisms/MainPage';
import Api from '../Api';
import MainPageData from '../MainPageData';
import GroupData from '../GroupData';
import MessageModel from '../MessageModel';
import { v4 as uuidv4 } from 'uuid';

const config = {
authority: "https://localhost:44370",
client_id: "js",
redirect_uri: "https://localhost:44370/callback.html",
response_type: "id_token token",
scope: "openid profile api1",
post_logout_redirect_uri: "https://localhost:44370",
};

export default function Messenger() {
  const [userManager, setUserManager] = useState(new Oidc.UserManager(config));
  const [isLogin, setIsLogin] = useState(false);
  const [mainPageData, setMainPageData] = useState(new MainPageData());
  const [groupData, setGroupData] = useState(new GroupData());
  const [showGroupInfo, setShowGroupInfo] = useState(false);
  const [foundUsers, setFoundUsers] = useState([]);
  const [renderNewMemberModal, setRenderNewMemberModal] = useState(false);
  // const [api, setApi] = useState(null);
  const api = useRef(new Api());
  useEffect(() => {
    async function someFun() {
      setIsLogin((await userManager.getUser().then(async (user) => {
        if (user) {
          await api.current.connectToHubs(user.access_token, setMainPageData, setGroupData, setFoundUsers);
          api.current.sendFirstData();
          // setApi({api: new Api(user.access_token)})
          return true;
        }
        else {
          return false;
        }
      })
      ))
    }
    someFun();
  }, []);
  function handleClickRenderNewMemberModal() {
    setRenderNewMemberModal(prevRenderNewMemberModal => !prevRenderNewMemberModal);
  }
  function handleSelectedGroupOnClick(groupId) {
    api.current.sendGroupData(groupId);
  }
  function handleSubmitSendMessage(event, message) {
    // message.simpleUserModel = null;
    message.id = uuidv4();
    // message.sendDate = new Date();
    message.sendDate = new Date(Date.now());
    setGroupData(prevGroupData => {
      prevGroupData.messages.push(message);
      return {...prevGroupData, messages: prevGroupData.messages};
    });
    if (message.value.length > 0) {
      api.current.sendMessage(message);
      event.target.reset();
    }
    event.preventDefault();
  }
  // function receiveMessage() {
  //   console.log("some text");
  //   console.log(api);
  //   console.log(api.current.foo);
  //   console.log(api.sendFirstData);
  //   console.log(api.valueConnection);
  //   console.log(api.createConnection);
  //   // console.log(api);
  //   api.current.foo();
  // }
  // checkLogin() {
  //   this.state.userManager.getUser().then((user) => {
  //     if (user) {
  //       this.setState({ isLogin: true });
  //       this.setState({ playerName: user.profile.name });
  //       this.setState({ fileMaster: new FileMaster(user.access_token) });
  //       this.setState({ api: new Api(user.access_token) });
  //     }
  //     else {
  //     }
  //   });
  // }
  function handleClickShowGroupInfo(){
    setShowGroupInfo(prevShowGroupInfo => !prevShowGroupInfo);
  }
  function handleClickNewMember() {
    
  }
  function handleChangeNewMemberModal(e) {
    console.log(e.target.value);
    api.current.searchUsers(e.target.value);
  }
  console.log(foundUsers)
  return (
    <div>
      {/* <p onClick={() => userManager.getUser().then((user) =>{
        if (user) {
          console.log("isLogin");
        }
        else {
          console.log("notLogin");
        }
      })}>is login?</p> */}
      <Navbar isLogin={isLogin} userManager={userManager} mainPageData={mainPageData}/>
      <MainPage
        api={api}
        mainPageData={mainPageData}
        groupData={groupData}
        selectedGroupOnClick={handleSelectedGroupOnClick}
        onSubmitSendMessage={handleSubmitSendMessage}
        showGroupInfo={showGroupInfo}
        onClickShowGroupInfo={handleClickShowGroupInfo}
        onClickNewMember={handleClickNewMember}
        foundUsers={foundUsers}
        onChangeNewMemberModal={handleChangeNewMemberModal}
        renderNewMemberModal={renderNewMemberModal}
        onClickRenderNewMemberModal={handleClickRenderNewMemberModal}
      />
      {/* <button
        onClick={receiveMessage}>
        get first data23
      </button>
      <button
        onClick={api ? api.foo : console.log("fuck")}>
        get first data23
      </button> */}
      {/* <p onClick={console.log(userManager)}>good job</p> */}
    </div>
  );
}