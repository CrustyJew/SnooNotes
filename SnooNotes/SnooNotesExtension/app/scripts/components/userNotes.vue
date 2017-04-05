<template>
<span>

    <span @click="show">[SN]</span>
    <transition name="fade">
    <div class="SNNotesDisplay" v-if="showNotes" :style="displayStyle" v-draggable="'.SNHeader'">
        <div class="SNHeader"><a class="SNCloseNote SNClose" @click="close">[x]</a></div>
        <table v-if="!notes.noNotes">
            <tr v-for="note in notes" :style="noteTypeStyle(note.SubName, note.NoteTypeID)">
                <td class="SNSubName">
                    <a :href="'https://reddit.com/r/'+note.SubName">{{note.SubName}}</a>
                    <span v-if="note.ParentSubreddit">
                        <br />via<br />
                        <a :href="'https://reddit.com/r/'+note.ParentSubreddit">{{note.ParentSubreddit}}</a>
                    </span>
                </td>
                <td class="SNSubmitter">
                    <span>{{note.Submitter}}</span>
                    <br />
                    <a :href="note.Url" style="white-space:pre;">{{new Date(note.Timestamp).toLocaleString().replace(', ', '\n')}}</a>
                </td>
                <td class="SNMessage">
                    <p>{{note.Message}}</p>
                    <a class="SNDeleteNote" v-if="!note.ParentSubreddit || modSubs.findIndex(s=>s.SubName == note.ParentSubreddit) > -1">[x]</a>
                </td>
            </tr>
        </table>
        <div class="SNNewNoteContainer">
            <div class="SNNewNote">
                <select class="SNNewNoteSub" v-model="newNoteSubIndex">
                    <option value="-1" disabled>--Select a Sub--</option>
                    <option :value="newNoteSubIndex" v-if="isModdedSub">{{subreddit}}</option>
                    <option value="-2" v-if="isModdedSub" disabled>---------</option>
                    <option v-for="sub in otherSubs" v-if="otherSubs.length >0" :value="sub.index">{{sub.name}}</option>
                </select>
                <textarea placeholder="Add a new note for user..." class="SNNewMessage" v-model="newNote" />
                <button type="button" class="SNBtnSubmit SNNewNoteSubmit" @click="submit">Submit</button>
            </div>
            <div class="SNNoteType">
                <label class="SNTypeRadio" 
                    v-for="nt in noteTypes" 
                    :style="noteTypeStyle(newNoteSubIndex,nt.NoteTypeID)"
                ><input type="radio" name="SNType" :value="nt.NoteTypeID" v-model="newNoteTypeID">{{nt.DisplayName}}</label>
            </div>
            <div class="SNNewError"></div>
        </div>
    </div>
</transition>
</span>
</template>
<script>
import {store} from '../redux/contentScriptStore';
import {draggable} from './directives/draggable';
    export default {
        name: 'user-notes',
        props:['username','subreddit','type','thingid'],
        directives:{'draggable':draggable},
        data(){
            return{
                message: this.username,
                snInfo: this.$select('snoonotes_info as snInfo'),
                userNotes: this.$select('notes.'+this.username+' as userNotes'),
                showNotes: false,
                displayStyle:{
                    display:'none',
                    position:'absolute',
                    top:'0px',
                    right:'0px',

                },
                newNote:"",
                newNoteTypeID: -1,
                newNoteSubIndex: -1
            }
        },
        computed:{
            hasNotes: function() {
                return this.usersWithNotes.indexOf(this.username) > -1;
            },
            usersWithNotes: function(){
                return this.snInfo.users_with_notes;
            },
            modSubs: function(){
                return this.snInfo.modded_subs.map((s,i)=>{return {name:s.SubName,id:s.SubredditID,index:i}});
            },
            otherSubs:function(){
                return this.modSubs.filter(sub=>sub.name != this.subreddit);
            },
            notes: function(){
                return this.userNotes || {noNotes:true}
            },
            noteTypes: function(){
                if(this.newNoteSubIndex == -1) return {}
                return this.snInfo.modded_subs[this.newNoteSubIndex].Settings.NoteTypes;
            },
            isModdedSub:function(){
                if(this.subreddit && !this.modSubs.findIndex(sub=> sub.name == this.subreddit) > -1){
                    return false;
                }
                return true;
            }
        },
        methods:{
            close: function(){
                this.showNotes = false;
                this.newNote="";
                this.newNoteTypeID = -1;
                document.removeEventListener('click',this.clickWatch,true);
            },
            noteTypeStyle: function(sub,ntID){
                let subIndex = isNaN(sub) ? this.snInfo.modded_subs.findIndex(subreddit=>subreddit.SubName == sub) : sub;
                let nt = this.snInfo.modded_subs[subIndex].Settings.NoteTypes.filter(nt=>nt.NoteTypeID == ntID )[0]
                let style = {
                    color: '#' + nt.ColorCode
                }
                if (nt.Bold) style.fontWeight = "bold";
                if (nt.Italic) style.fontStyle = "italic";
                return style;
            },
            show:function(e){
                this.displayStyle.top = e.pageY +'px';
                this.displayStyle.left = e.pageX + 'px';
                this.displayStyle.display = 'block';
                this.showNotes = true;
                if(this.subreddit){
                    this.newNoteSubIndex = this.modSubs.findIndex(sub => sub.name == this.subreddit)
                }else{
                   this.newNoteSubIndex = -1
                }
                document.addEventListener('click',this.clickWatch,true);
            },
            clickWatch: function(e){
                if(!this.$el.contains(e.target)) this.close();
            }
        },
        beforeDestroy:function(){
            document.removeEventListener('click',this.clickWatch,true);
        }
    }
</script>
<style lang="scss">

@import "~styles/_vars.scss";
.SNNotesDisplay{
.SNHeader{
    margin-left:-10px;
    margin-right:-10px;
.SNClose{
  display:inline-block;
  cursor: pointer;
  color:white;
  background-color: $accent;
  font-weight:bold;
  padding: 2px 10px;
  margin-top: -1px;
  &:hover{
        background-image: linear-gradient(to bottom, darken($accent,10%),darken($accent,25%));
    }
}
}
}
</style>