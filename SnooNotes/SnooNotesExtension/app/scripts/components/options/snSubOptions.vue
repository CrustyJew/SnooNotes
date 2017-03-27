<template>
<div id="SNSubRedditSettings" >
  <div class="SNOptsHeader"><h1 class="SNSubOptsHeader"></h1><button type="button" class="SNBtnCancel" id="SNBtnSubOptsCancel" @click="cancel">Cancel</button><br style="clear:both;"></div>
  <div class="SNContainer">
    <div id="SNAccessMask">
      <div id="SNAccessMaskDesc">Choose who can view and add notes below. Anyone with full permissions can always view and add notes as well as edit this page</div>
      <div id="SNAccessMaskOptions">
        <label><input type="checkbox" value="1" v-model.number="selectedAccess">access</label>
        <label><input type="checkbox" value="2" v-model.number="selectedAccess">config</label>
        <label><input type="checkbox" value="4" v-model.number="selectedAccess">flair</label>
        <label><input type="checkbox" value="8" v-model.number="selectedAccess">mail</label>
        <label><input type="checkbox" value="16" v-model.number="selectedAccess">posts</label>
        <label><input type="checkbox" value="32" v-model.number="selectedAccess">wiki</label>
      </div>
    </div>
    <div id="SNNoteTypes">
      <div id="SNNoteTypesDesc">Change just about everything about the Note Types belonging to this subreddit below. If no checkbox is chosen for Perm Ban or Temp Ban, then automatic ban notes will not be generated for that type of ban.<br /><br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Temp&nbsp;|&nbsp;Perm<br />&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Ban&nbsp;&nbsp;|&nbsp;&nbsp;Ban</div>
      <ol>
          <li :sn-notetype-id="nt.NoteTypeID" :sn-notetype-display-order="nt.DisplayOrder" v-for="nt in sub.Settings.NoteTypes">
              <a class="SNSort"></a>>
              <input type="checkbox" class="SNChkGrp" :value="nt.NoteTypeID" v-model.number="tempBanID">
              <input type="checkbox" class="SNChkGrp" :value="nt.NoteTypeID" v-model.number="permBanID">
              <input class="SNNoteTypeDisp" type="text" maxlength="20" v-model="nt.DisplayName">
              &nbsp;Color:&nbsp;<input class="SNNoteTypeColor" type="color" v-model="nt.ColorCode">
              <label><input type="checkbox" class="SNntBold" v-model="nt.Bold">Bold</label>
              <label><input type="checkbox" class="SNntItalic" v-model="nt.Italic">Italic</label>
          </li>
      </ol>
    </div>
  </div>
  <button type="button" class="SNBtnSubmit" id="SNBtnSubOptsSave" @click="save">Save</button></div>
</div>
</template>
<script>

export default {
    name:"SNSubOptions",
    props:["sub","cancel"],
    data(){
        return {
            selectedAccess: [],
            permBanID:[],
            tempBanID: []
        }
    },
    methods:{
        selectPermBan(id){
            this.permBanID.clear();
            this.permBanID.push(id);
        },
        selectTempBan(id){
            this.tempBanID.clear();
            this.tempBanID.push(id);
        }
    },
    created: function(){
        if(this.sub.Settings.AccessMask & 0x01) this.selectedAccess.push(1);
        if(this.sub.Settings.AccessMask & 0x02) this.selectedAccess.push(2);
        if(this.sub.Settings.AccessMask & 0x04) this.selectedAccess.push(4);
        if(this.sub.Settings.AccessMask & 0x08) this.selectedAccess.push(8);
        if(this.sub.Settings.AccessMask & 0x10) this.selectedAccess.push(16);
        if(this.sub.Settings.AccessMask & 0x20) this.selectedAccess.push(32);
        if(this.sub.Settings.PermBanID) this.permBanID.push(this.sub.Settings.PermBanID);
        if(this.sub.Settings.TempBanID) this.tempBanID.push(this.sub.Settings.TempBanID);
    }
}
</script>
<style lang="scss">
#SNAccessMask {
    width: 200px;
    display: inline-block;
    border: 1px solid transparent;
    border-radius: 5px;
    background-color: lightgrey;
    padding: 5px;
    margin-right: 20px;
    margin-bottom: 20px;
    vertical-align: top;
}
#SNAccessMaskOptions {
    width: 100px;
    label {
        display: block;
    }
    input[type=checkbox] {
        vertical-align: middle;
        margin: 0px 5px 0px 0px;
    }
}
#SNAccessMaskDesc {
    font-weight: bold;
    margin-bottom: 5px;
}

#SNNoteTypesDesc {
    font-weight: bold;
}
#SNSubRedditSettings .SNNoteTypes {
    display: inline-block;
    width: 650px;
    padding: 5px;
    border: 1px solid transparent;
    background-color: lightgrey;
    border-radius: 5px;

    li {
        cursor: grab;
        cursor: -webkit-grab;
        border: 1px solid darkgrey;
        border-radius: 3px;
        padding: 2px;
        background-color: lightgrey;
        margin: 1px;

        &:active{
            cursor:grabbing;
            cursor:-webkit-grabbing;
        }
    }

    input{
        vertical-align:middle;
    }

    input[type=checkbox] {
        vertical-align: middle;
        margin: 0px 5px 0px 5px;
        height: 20px;
    }

    a.SNSort {
        display:inline-block;
        vertical-align:middle;
        height: 6px; /*length of Arrow*/
        width: 4px;
        margin:0 8px;
        background-color: black;
        position: relative;

        &:before{
            content: "";
            position: absolute;
            left: -4px;
            top: -6px;
            border-color: black transparent;
            border-style: solid;
            border-width: 0px 6px 6px 6px;
            height: 0px;
            width: 0px;
        }

        &:after {
            content: "";
            position: absolute;
            left: -4px;
            top: 100%;
            border-color: black transparent;
            border-style: solid;
            border-width: 6px 6px 0px 6px;
            height: 0px;
            width: 0px;
        }
    }
}
.SNPreview{
    display:inline-block;
    min-width:160px;
}
</style>