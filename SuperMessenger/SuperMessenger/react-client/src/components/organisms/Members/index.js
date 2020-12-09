import React from 'react';
import GroupType from '../../../containers/Enums/GroupType';
import Div from '../../atoms/Div';
import Input from '../../atoms/Input';
import SimpleContent from '../SimpleContent';

import styles from './style.module.css'

export default function Members(props) {
  return (
    <Div className="column row m-1 p-0 flex-column">
      {
        (props.isCreator || props.groupType == GroupType.public) &&
        <Input className="column addMemberInput" type="button" defaultValue="add new member" onClick={props.onClickAddMember} />
      }
      {
        props.usersInGroup &&
        props.usersInGroup.map(userInGroup => 
          <SimpleContent
              id={props.groupId}
              key={userInGroup.id}
              simpleContentClasses="simpleGroupContent"
              imgContentClasses="simpleImgContent"
              imgClasses="simpleImg" 
              isUser={true}
              imageId={userInGroup.imageId}
              name={userInGroup.email}
          />
          )
      }
    </Div>
  );
}