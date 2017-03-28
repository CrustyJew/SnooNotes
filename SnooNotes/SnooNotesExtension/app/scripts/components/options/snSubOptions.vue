<template>
<div id="SNSubRedditSettings" >
  <div class="SNOptsHeader"><h1 class="SNSubOptsHeader">/r/{{initialSettings.SubName}}</h1><button type="button" class="SNBtnCancel" id="SNBtnSubOptsCancel" @click="cancel">Cancel</button><br style="clear:both;"></div>
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
      <draggable element="ul" :list="newSettings.Settings.NoteTypes" :options="dragOptions">
          <li :sn-notetype-id="nt.NoteTypeID" :sn-notetype-display-order="nt.DisplayOrder" v-for="nt in newSettings.Settings.NoteTypes">
              <a class="SNSort"></a>
              <input type="checkbox" class="SNChkGrp" :value="nt.NoteTypeID" v-on:change="selectTempBan(nt.NoteTypeID, $event)" v-model.number="tempBanID">
              <input type="checkbox" class="SNChkGrp" :value="nt.NoteTypeID" v-on:change="selectPermBan(nt.NoteTypeID, $event)" v-model.number="permBanID">
              <input class="SNNoteTypeDisp" type="text" maxlength="20" v-model="nt.DisplayName">
              &nbsp;Color:&nbsp;<input class="SNNoteTypeColor" type="color" :value="nt.ColorCode" v-model="nt.ColorCode">
              <label><input type="checkbox" class="SNntBold" v-model="nt.Bold">Bold</label>
              <label><input type="checkbox" class="SNntItalic" v-model="nt.Italic">Italic</label>
              &nbsp;<span class="SNPreview" :style="getStyle(nt)">{{nt.DisplayName}}</span>
              <a class="SNRemove" @click="removeNoteType(nt)">x</a>
          </li>
      </draggable>
      <div style="text-align:center;" @click="addNoteType"><a class="SNAdd">+</a></div>
    </div>
  </div>
  <button type="button" class="SNBtnSubmit" id="SNBtnSubOptsSave" @click="save" :disabled="saving">{{saving ? "Saving..." : "Save"}}</button>
  </div>

</template>
<script>
import draggable from 'vuedraggable'
import axios from 'axios';
import Toasted from 'vue-toasted';
export default {
    name:"SNSubOptions",
    props:["sub","cancel","finish"],
    components: {
          draggable,
          Toasted
    },
    data(){
        return {
            selectedAccess: [],
            permBanID:[],
            tempBanID: [],
            initialSettings: this.sub,
            newSettings: {},
            saving:false
        }
    },
    methods:{
        selectPermBan(id, e){
            if(e.target.checked){
                this.permBanID = [];
                this.permBanID.push(id);
            }
        },
        selectTempBan(id,e){
            if(e.target.checked){
                this.tempBanID = [];
                this.tempBanID.push(id);
            }
        },
        getStyle(nt){
            return{
                color: nt.ColorCode,
                fontWeight: nt.Bold ? 'bold' : 'normal',
                fontStyle: nt.Italic ? 'italic' : 'normal'
            }
        },
        addNoteType(){
            this.newSettings.Settings.NoteTypes.push({
                DisplayName:'',
                ColorCode: '#000000',
                Bold: false,
                Italic: false
            })
        },
        removeNoteType(nt){
            this.newSettings.Settings.NoteTypes.splice(this.newSettings.Settings.NoteTypes.indexOf(nt),1);
        },
        save(){
            this.saving = true;
            let newAccessMask = 64;
            for(let i = 0; i < this.selectedAccess.length; i++){
                newAccessMask += this.selectedAccess[i];
            }

            if(newAccessMask != this.initialSettings.Settings.AccessMask
               || this.initialSettings.Settings.PermBanID != this.permBanID[0]
               || this.initialSettings.Settings.TempBanID != this.tempBanID[0]){
                
                //access mask, or auto ban note id changed
                axios.put('subreddit/'+ this.initialSettings.SubName,{
                    Settings:{
                        AccessMask: newAccessMask,
                        TempBanID: this.tempBanID[0],
                        PermBanID: this.permBanID[0]
                    }
                }).then(()=>{
                    this.$toasted.success("Saved Settings!");
                    this.saving = false;
                    this.finish();
                },()=>{
                    this.$toasted.error("Failed to save settings!",{duration:null});
                    this.saving = false;
                })
            }
        }
    },
    computed:{
        dragOptions(){
            return {
                animation:150,
                ghostClass: 'ghost'
            }
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
        for(var i = 0; i < this.sub.Settings.NoteTypes.length; i++){
            if(this.sub.Settings.NoteTypes[i].ColorCode.indexOf('#') == -1){
                this.sub.Settings.NoteTypes[i].ColorCode = '#' + this.sub.Settings.NoteTypes[i].ColorCode;
            }
        }
        this.newSettings = JSON.parse(JSON.stringify(this.sub));
    }
}
</script>
<style lang="scss">
@import "~styles/_vars.scss";

.ghost {
  opacity: .5;
  background: $light-gray;
}
#SNSubRedditSettings{
    line-height: 12px;
    text-align: left;
}
.SNSubOptsHeader {
    float: left;
    color: $secondary;
    font-weight: bold;
    margin: 0px;
}
.SNOptsHeader {
    margin-bottom: 10px;
}
#SNBtnSubOptsCancel {
    float: right;
}
#SNBtnSubOptsSave {
    margin: 0 auto;
    margin-top: 20px;
    display: block;
}
#SNAccessMask {
    width: 200px;
    display: inline-block;
    border: 1px solid transparent;
    border-radius: 5px;
    background-color: $light-gray;
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
#SNNoteTypes {
    display: inline-block;
    width: 650px;
    padding: 5px;
    border: 1px solid transparent;
    background-color: $light-gray;
    border-radius: 5px;

    li {
        cursor: grab;
        cursor: -webkit-grab;
        border: 1px solid darkgrey;
        border-radius: 3px;
        padding: 2px;
        background-color: $light-gray;
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
.SNRemove {
    display:inline-block;
    vertical-align:middle;
    height: 16px;
    width: 16px;
    font-size:12px;
    line-height:14px;
    font-weight:bold;
    font-family:verdana;
    color:white;
    text-align:center;
    margin-left:8px;
    background: linear-gradient(to bottom, $accent, darken($accent,15%));
    position: relative;
    border-radius:50%;
    border:1px solid darkgrey;
    cursor:pointer;
}
.SNAdd {
    display:inline-block;
    vertical-align:middle;
    height: 16px;
    width: 16px;
    font-size:12px;
    line-height:15px;
    font-weight:bold;
    font-family:verdana;
    color:white;
    text-align:center;
    margin-left:8px;
    background: linear-gradient(to bottom, #74c429, #4ca20b );
    position: relative;
    border-radius:50%;
    border:1px solid darkgrey;
    cursor:pointer;
}
</style>